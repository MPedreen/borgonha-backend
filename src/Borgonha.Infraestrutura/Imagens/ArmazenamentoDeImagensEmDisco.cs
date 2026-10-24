using System.Text.RegularExpressions;
using Borgonha.Aplicacao.Administracao;
using SkiaSharp;

namespace Borgonha.Infraestrutura.Imagens;

/// <summary>
/// Guarda em disco as fotos enviadas pelo administrador, otimizadas como as fotos do site:
/// WebP nas mesmas larguras do front-end (frontend/src/app/core/imagens/larguras-imagem.ts).
/// A foto é decodificada e gravada de novo, então nada do arquivo original (metadados como a
/// localização do celular, ou conteúdo malicioso disfarçado de imagem) chega ao site.
/// </summary>
internal sealed partial class ArmazenamentoDeImagensEmDisco(OpcoesDasImagens opcoes) : IArmazenamentoDeImagens
{
    public const string PastaDosEnvios = "envios";

    private const long TamanhoMaximoEmBytes = 15 * 1024 * 1024;
    private const long MaximoDePixels = 50_000_000;
    private const int QualidadeWebp = 82;

    private static readonly int[] Larguras = [160, 320, 480, 640, 960, 1280, 1600];

    private static readonly SKEncodedImageFormat[] FormatosAceitos =
        [SKEncodedImageFormat.Jpeg, SKEncodedImageFormat.Png, SKEncodedImageFormat.Webp, SKEncodedImageFormat.Heif];

    private string PastaDeEnvios => Path.Combine(opcoes.Pasta, PastaDosEnvios);

    public async Task<string> SalvarAsync(Stream conteudo, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var bytes = await LerComLimiteAsync(conteudo, cancellationToken).ConfigureAwait(false);
        using var foto = Decodificar(bytes);

        var id = Guid.NewGuid().ToString("N");
        Directory.CreateDirectory(PastaDeEnvios);

        try
        {
            foreach (var largura in Larguras)
            {
                var webp = GerarWebp(foto, largura);
                await File.WriteAllBytesAsync(Path.Combine(PastaDeEnvios, $"{id}-{largura}.webp"), webp, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch
        {
            ApagarArquivos(id);
            throw;
        }

        return $"{PastaDosEnvios}/{id}";
    }

    public Task RemoverAsync(string caminho, CancellationToken cancellationToken)
    {
        var envio = CaminhoDeEnvio().Match(caminho ?? string.Empty);
        if (envio.Success)
        {
            ApagarArquivos(envio.Groups["id"].Value);
        }

        return Task.CompletedTask;
    }

    private static async Task<byte[]> LerComLimiteAsync(Stream conteudo, CancellationToken cancellationToken)
    {
        using var memoria = new MemoryStream();
        var bloco = new byte[81_920];
        int lidos;

        while ((lidos = await conteudo.ReadAsync(bloco, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (memoria.Length + lidos > TamanhoMaximoEmBytes)
            {
                throw new ImagemInvalidaException($"A foto deve ter no máximo {TamanhoMaximoEmBytes / 1024 / 1024} MB.");
            }

            memoria.Write(bloco, 0, lidos);
        }

        return memoria.ToArray();
    }

    private static SKBitmap Decodificar(byte[] bytes)
    {
        using var fluxo = new MemoryStream(bytes, writable: false);
        using var codec = SKCodec.Create(fluxo);

        if (codec is null || !FormatosAceitos.Contains(codec.EncodedFormat))
        {
            throw new ImagemInvalidaException("O arquivo enviado não é uma foto aceita. Envie uma imagem JPG, PNG, WebP ou HEIC.");
        }

        if ((long)codec.Info.Width * codec.Info.Height > MaximoDePixels)
        {
            throw new ImagemInvalidaException("A foto tem resolução alta demais. Envie uma foto de até 50 megapixels.");
        }

        var foto = SKBitmap.Decode(codec)
            ?? throw new ImagemInvalidaException("Não foi possível abrir a foto. Tente enviar outra imagem.");

        return Endireitar(foto, codec.EncodedOrigin);
    }

    /// <summary>
    /// Aplica a orientação gravada pela câmera (EXIF), para a foto não aparecer deitada.
    /// </summary>
    private static SKBitmap Endireitar(SKBitmap foto, SKEncodedOrigin origem)
    {
        if (origem == SKEncodedOrigin.TopLeft)
        {
            return foto;
        }

        float w = foto.Width;
        float h = foto.Height;
        var matriz = origem switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            _ => SKMatrix.Identity,
        };

        var deitada = origem is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var endireitada = new SKBitmap(
            deitada ? foto.Height : foto.Width,
            deitada ? foto.Width : foto.Height,
            foto.ColorType,
            foto.AlphaType);

        using (foto)
        using (var canvas = new SKCanvas(endireitada))
        using (var imagem = SKImage.FromBitmap(foto))
        {
            canvas.SetMatrix(matriz);
            // Giros de 90° caem em pixels inteiros: não precisa de interpolação.
            canvas.DrawImage(imagem, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        }

        return endireitada;
    }

    /// <summary>
    /// WebP na largura pedida, sem ampliar fotos menores (nesse caso, sai na largura original).
    /// </summary>
    private static byte[] GerarWebp(SKBitmap foto, int largura)
    {
        var larguraFinal = Math.Min(largura, foto.Width);
        var alturaFinal = Math.Max(1, (int)Math.Round(foto.Height * (larguraFinal / (double)foto.Width)));

        using var redimensionada = larguraFinal == foto.Width
            ? null
            : foto.Resize(new SKImageInfo(larguraFinal, alturaFinal, foto.ColorType, foto.AlphaType), new SKSamplingOptions(SKCubicResampler.Mitchell));
        using var imagem = SKImage.FromBitmap(redimensionada ?? foto);
        using var dados = imagem.Encode(SKEncodedImageFormat.Webp, QualidadeWebp)
            ?? throw new InvalidOperationException("Não foi possível gerar o WebP da foto.");

        return dados.ToArray();
    }

    private void ApagarArquivos(string id)
    {
        if (!Directory.Exists(PastaDeEnvios))
        {
            return;
        }

        foreach (var arquivo in Directory.EnumerateFiles(PastaDeEnvios, $"{id}-*.webp"))
        {
            File.Delete(arquivo);
        }
    }

    [GeneratedRegex("^envios/(?<id>[0-9a-f]{32})$")]
    private static partial Regex CaminhoDeEnvio();
}
