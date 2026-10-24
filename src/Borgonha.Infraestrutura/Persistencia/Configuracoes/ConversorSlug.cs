using Borgonha.Dominio.Comum;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Borgonha.Infraestrutura.Persistencia.Configuracoes;

internal sealed class ConversorSlug() : ValueConverter<Slug, string>(slug => slug.Valor, valor => Slug.Criar(valor));
