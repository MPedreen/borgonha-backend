using Scalar.AspNetCore;

namespace Borgonha.Api.Configuracao;

internal static class PipelineDaApi
{
    public static WebApplication UsarApi(this WebApplication app)
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
        app.UseCors();
        app.UseOutputCache();

        return app;
    }
}
