using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borgonha.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class TextoDaDuvidaDeEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A regra passou a medir o trajeto de entrega, não a linha reta.
            migrationBuilder.Sql(
                """
                UPDATE duvidas SET resposta = 'Depende da distância. Até 1,5 km da loja, pelo trajeto de entrega, não tem pedido mínimo. Mais longe que isso, o pedido mínimo é de R$ 30. Na seção “Entrega” você digita seu endereço e confere na hora.'
                WHERE pergunta = 'Tem pedido mínimo para entrega?' AND resposta = 'Depende da distância. Até 1,5 km da loja, em linha reta, não tem pedido mínimo. Mais longe que isso, o pedido mínimo é de R$ 30. Na seção “Entrega” você digita sua rua e confere na hora.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE duvidas SET resposta = 'Depende da distância. Até 1,5 km da loja, em linha reta, não tem pedido mínimo. Mais longe que isso, o pedido mínimo é de R$ 30. Na seção “Entrega” você digita sua rua e confere na hora.'
                WHERE pergunta = 'Tem pedido mínimo para entrega?' AND resposta = 'Depende da distância. Até 1,5 km da loja, pelo trajeto de entrega, não tem pedido mínimo. Mais longe que isso, o pedido mínimo é de R$ 30. Na seção “Entrega” você digita seu endereço e confere na hora.';
                """);
        }
    }
}
