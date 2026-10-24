using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

public sealed record Coordenadas
{
    public Coordenadas(double latitude, double longitude)
    {
        Latitude = latitude is >= -90 and <= 90
            ? latitude
            : throw new DominioException("A latitude deve estar entre -90 e 90.");
        Longitude = longitude is >= -180 and <= 180
            ? longitude
            : throw new DominioException("A longitude deve estar entre -180 e 180.");
    }

    public double Latitude { get; }

    public double Longitude { get; }
}
