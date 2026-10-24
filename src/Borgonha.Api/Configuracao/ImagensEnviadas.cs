using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

namespace Borgonha.Api.Configuracao;

/// <summary>
/// Publica as fotos enviadas pela área do administrador em /api/imagens/envios/{id}-{largura}.webp.
/// Só arquivos WebP são servidos, e como cada envio ganha um nome novo, o navegador pode guardá-los para sempre.
/// </summary>
internal static class ImagensEnviadas
{
    public const string Caminho = "/api/imagens";

    public static WebApplication UsarImagensEnviadas(this WebApplication app, string pastaDasImagens)
    {
        Directory.CreateDirectory(pastaDasImagens);

        app.UseStaticFiles(new StaticFileOptions
        {
            RequestPath = Caminho,
            FileProvider = new PhysicalFileProvider(pastaDasImagens),
            ContentTypeProvider = new FileExtensionContentTypeProvider(new Dictionary<string, string> { [".webp"] = "image/webp" }),
            ServeUnknownFileTypes = false,
            OnPrepareResponse = contexto =>
            {
                var cabecalhos = contexto.Context.Response.Headers;
                cabecalhos.CacheControl = "public, max-age=31536000, immutable";
                cabecalhos[HeaderNames.XContentTypeOptions] = "nosniff";
            },
        });

        return app;
    }
}
