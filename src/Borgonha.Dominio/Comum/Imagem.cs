using System.Text.RegularExpressions;

namespace Borgonha.Dominio.Comum;

/// <summary>
/// Foto de um produto: caminho base dos arquivos otimizados e texto alternativo para acessibilidade.
/// </summary>
public sealed partial record Imagem
{
    public const int TamanhoMaximoCaminho = 120;
    public const int TamanhoMaximoTextoAlternativo = 200;

    public Imagem(string caminho, string textoAlternativo)
    {
        caminho = Garantir.TextoObrigatorio(caminho, "Caminho da imagem", TamanhoMaximoCaminho);

        if (!FormatoCaminho().IsMatch(caminho))
        {
            throw new DominioException($"Caminho de imagem inválido: '{caminho}'.");
        }

        Caminho = caminho;
        TextoAlternativo = Garantir.TextoObrigatorio(textoAlternativo, "Texto alternativo da imagem", TamanhoMaximoTextoAlternativo);
    }

    /// <summary>
    /// Caminho relativo, sem largura nem extensão (ex.: "cookies/nutella-partido").
    /// O front-end monta o arquivo final de acordo com o tamanho da tela.
    /// </summary>
    public string Caminho { get; }

    public string TextoAlternativo { get; }

    [GeneratedRegex("^[a-z0-9-]+(?:/[a-z0-9-]+)*$")]
    private static partial Regex FormatoCaminho();
}
