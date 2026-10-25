using Scalar.AspNetCore;

namespace Borgonha.Api.Configuracao;

internal static class PipelineDaApi
{
    public static WebApplication UsarApi(this WebApplication app, string pastaDasImagens)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseResponseCompression();
        app.UsarImagensEnviadas(pastaDasImagens);
        app.UseCors();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOutputCache();

        return app;
    }
}
