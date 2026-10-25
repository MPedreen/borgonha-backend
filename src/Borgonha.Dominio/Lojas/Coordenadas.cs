using Borgonha.Dominio.Comum;

namespace Borgonha.Dominio.Lojas;

public sealed record Coordenadas
{
    private const double RaioDaTerraEmMetros = 6_371_008.8;

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

    /// <summary>Distância em linha reta até outro ponto, em metros (fórmula de haversine).</summary>
    public double DistanciaEmLinhaRetaAte(Coordenadas outra)
    {
        ArgumentNullException.ThrowIfNull(outra);

        var latitudeA = Radianos(Latitude);
        var latitudeB = Radianos(outra.Latitude);
        var seno = Math.Sin((latitudeB - latitudeA) / 2);
        var senoLongitude = Math.Sin(Radianos(outra.Longitude - Longitude) / 2);
        var h = (seno * seno) + (Math.Cos(latitudeA) * Math.Cos(latitudeB) * senoLongitude * senoLongitude);

        return 2 * RaioDaTerraEmMetros * Math.Asin(Math.Sqrt(h));
    }

    private static double Radianos(double graus) => graus * Math.PI / 180;
}
