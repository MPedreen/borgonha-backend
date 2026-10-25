namespace Borgonha.Aplicacao.Entrega;

/// <param name="Titulo">Rua e número, para mostrar em destaque.</param>
/// <param name="Detalhe">Bairro, cidade e estado.</param>
public sealed record EnderecoSugeridoDto(string Titulo, string Detalhe, double Latitude, double Longitude);

/// <param name="Situacao">"sem-minimo", "com-minimo" ou "incerta".</param>
/// <param name="DistanciaEmMetros">Distância do trajeto de entrega; nula quando a linha reta já bastou para decidir.</param>
/// <param name="DistanciaEmLinhaRetaEmMetros">Distância em linha reta da loja até o endereço.</param>
public sealed record ResultadoDaEntregaDto(string Situacao, int? DistanciaEmMetros, int DistanciaEmLinhaRetaEmMetros);
