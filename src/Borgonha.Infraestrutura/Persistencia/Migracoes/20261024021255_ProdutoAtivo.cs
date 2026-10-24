using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borgonha.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class ProdutoAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Produtos",
                type: "INTEGER",
                nullable: false,
                // Os produtos que já existem continuam no site.
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Produtos");
        }
    }
}
