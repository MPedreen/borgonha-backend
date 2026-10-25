using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nome = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "duvidas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pergunta = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    resposta = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_duvidas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lojas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    logradouro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    bairro = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    cidade = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    uf = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    cep = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    telefone = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    instagram = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    aviso_topo = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lojas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    categoria_id = table.Column<int>(type: "integer", nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    descricao = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    frase = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    preco = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    peso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    selo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    variacoes = table.Column<string[]>(type: "text[]", nullable: false),
                    imagem_inteiro_caminho = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    imagem_inteiro_texto_alternativo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    imagem_partido_caminho = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    imagem_partido_texto_alternativo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    disponivel = table.Column<bool>(type: "boolean", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_produtos", x => x.id);
                    table.ForeignKey(
                        name: "fk_produtos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "canais_pedido",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    titulo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    descricao = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    mensagem_padrao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    selo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    destaque = table.Column<bool>(type: "boolean", nullable: false),
                    loja_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_canais_pedido", x => x.id);
                    table.ForeignKey(
                        name: "fk_canais_pedido_lojas_loja_id",
                        column: x => x.loja_id,
                        principalTable: "lojas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "horarios_funcionamento",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    dia_inicial = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    dia_final = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    abertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    fechamento = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    loja_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_horarios_funcionamento", x => x.id);
                    table.ForeignKey(
                        name: "fk_horarios_funcionamento_lojas_loja_id",
                        column: x => x.loja_id,
                        principalTable: "lojas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_canais_pedido_loja_id",
                table: "canais_pedido",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_slug",
                table: "categorias",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_horarios_funcionamento_loja_id",
                table: "horarios_funcionamento",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ix_produtos_slug",
                table: "produtos",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "canais_pedido");

            migrationBuilder.DropTable(
                name: "duvidas");

            migrationBuilder.DropTable(
                name: "horarios_funcionamento");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "lojas");

            migrationBuilder.DropTable(
                name: "categorias");
        }
    }
}
