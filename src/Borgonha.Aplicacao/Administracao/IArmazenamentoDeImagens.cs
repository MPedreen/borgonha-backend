namespace Borgonha.Aplicacao.Administracao;

/// <summary>
/// Guarda as fotos enviadas pela área do administrador.
/// </summary>
public interface IArmazenamentoDeImagens
{
    /// <summary>
    /// Confere se o arquivo é mesmo uma foto, gera as versões otimizadas (WebP nas larguras usadas
    /// pelo site, sem metadados como a localização do celular) e devolve o caminho base da foto
    /// (ex.: "envios/9f1c2b...").
    /// </summary>
    /// <exception cref="ImagemInvalidaException">O arquivo não é uma foto aceita ou é grande demais.</exception>
    Task<string> SalvarAsync(Stream conteudo, CancellationToken cancellationToken);

    /// <summary>
    /// Apaga os arquivos de uma foto enviada. Caminhos de fotos que não vieram da área do
    /// administrador (as que fazem parte do próprio site) são ignorados.
    /// </summary>
    Task RemoverAsync(string caminho, CancellationToken cancellationToken);
}
