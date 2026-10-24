using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borgonha.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class DescricaoOpcional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "TEXT",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 400);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Produtos",
                type: "TEXT",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 400,
                oldNullable: true);
        }
    }
}
