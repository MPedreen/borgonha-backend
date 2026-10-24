namespace Borgonha.Api.Endpoints;

internal static class EndpointsDaApi
{
    public static IEndpointRouteBuilder MapearEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");

        return app;
    }
}
