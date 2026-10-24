using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Borgonha.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Duvidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Pergunta = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Resposta = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duvidas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lojas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Numero = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Bairro = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Uf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 9, nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Instagram = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    AvisoTopo = table.Column<string>(type: "TEXT", maxLength: 140, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lojas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Frase = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    Preco = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: true),
                    Peso = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Selo = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Variacoes = table.Column<string>(type: "TEXT", nullable: false),
                    ImagemInteiroCaminho = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    ImagemInteiroTextoAlternativo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ImagemPartidoCaminho = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    ImagemPartidoTextoAlternativo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Disponivel = table.Column<bool>(type: "INTEGER", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CanaisPedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    MensagemPadrao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Selo = table.Column<string>(type: "TEXT", maxLength: 24, nullable: true),
                    Destaque = table.Column<bool>(type: "INTEGER", nullable: false),
                    LojaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanaisPedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CanaisPedido_Lojas_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Lojas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HorariosFuncionamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiaInicial = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DiaFinal = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Abertura = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Fechamento = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    LojaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosFuncionamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosFuncionamento_Lojas_LojaId",
                        column: x => x.LojaId,
                        principalTable: "Lojas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CanaisPedido_LojaId",
                table: "CanaisPedido",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Slug",
                table: "Categorias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosFuncionamento_LojaId",
                table: "HorariosFuncionamento",
                column: "LojaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_Slug",
                table: "Produtos",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CanaisPedido");

            migrationBuilder.DropTable(
                name: "Duvidas");

            migrationBuilder.DropTable(
                name: "HorariosFuncionamento");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Lojas");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
