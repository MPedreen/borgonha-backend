using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borgonha.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class RegraDeEntrega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "entrega_pedido_minimo",
                table: "lojas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "entrega_raio_sem_pedido_minimo_em_metros",
                table: "lojas",
                type: "integer",
                nullable: true);

            // Bancos que já tinham a loja e as dúvidas: num banco novo as tabelas ainda estão vazias aqui,
            // e os dados iniciais já trazem a regra e a dúvida.
            migrationBuilder.Sql(
                """
                UPDATE lojas
                SET entrega_raio_sem_pedido_minimo_em_metros = 1500, entrega_pedido_minimo = 30
                WHERE entrega_raio_sem_pedido_minimo_em_metros IS NULL;

                UPDATE duvidas SET ordem = ordem + 1
                WHERE ordem >= 2 AND NOT EXISTS (SELECT 1 FROM duvidas WHERE pergunta = 'Tem pedido mínimo para entrega?');

                INSERT INTO duvidas (pergunta, resposta, ordem)
                SELECT 'Tem pedido mínimo para entrega?',
                       'Depende da distância. Até 1,5 km da loja, em linha reta, não tem pedido mínimo. Mais longe que isso, o pedido mínimo é de R$ 30. Na seção “Entrega” você digita sua rua e confere na hora.',
                       2
                WHERE EXISTS (SELECT 1 FROM duvidas)
                  AND NOT EXISTS (SELECT 1 FROM duvidas WHERE pergunta = 'Tem pedido mínimo para entrega?');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM duvidas WHERE pergunta = 'Tem pedido mínimo para entrega?';
                UPDATE duvidas SET ordem = ordem - 1 WHERE ordem > 2;
                """);

            migrationBuilder.DropColumn(
                name: "entrega_pedido_minimo",
                table: "lojas");

            migrationBuilder.DropColumn(
                name: "entrega_raio_sem_pedido_minimo_em_metros",
                table: "lojas");
        }
    }
}
