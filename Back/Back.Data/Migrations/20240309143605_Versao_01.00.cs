using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AZURE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Token = table.Column<string>(maxLength: 255, nullable: true),
                    UrlCorporacao = table.Column<string>(maxLength: 255, nullable: true),
                    ProjetoNome = table.Column<string>(maxLength: 255, nullable: true),
                    ProjetoId = table.Column<string>(maxLength: 255, nullable: true),
                    TimeNome = table.Column<string>(maxLength: 255, nullable: true),
                    TimeId = table.Column<string>(maxLength: 255, nullable: true),
                    Principal = table.Column<bool>(type: "Bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AZURE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CONFIGURACOES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Dia = table.Column<int>(type: "Int", nullable: false),
                    HoraCron = table.Column<string>(maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONFIGURACOES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CONTAS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmailDe = table.Column<string>(maxLength: 255, nullable: true),
                    EmailPara = table.Column<string>(maxLength: 255, nullable: true),
                    AreaId = table.Column<long>(type: "Integer", nullable: false),
                    AreaPath = table.Column<string>(maxLength: 255, nullable: true),
                    Sprint = table.Column<string>(maxLength: 255, nullable: true),
                    Cliente = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTAS", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SINCRONIZACOES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataInicio = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DataFim = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(type: "Int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINCRONIZACOES", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SINCRONIZAR_ITENS",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Status = table.Column<string>(maxLength: 100, nullable: true),
                    Tipo = table.Column<string>(maxLength: 100, nullable: true),
                    Erro = table.Column<string>(nullable: true),
                    Origem = table.Column<int>(maxLength: 100, nullable: false),
                    Destino = table.Column<int>(maxLength: 100, nullable: false),
                    SincronizarId = table.Column<int>(type: "Integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINCRONIZAR_ITENS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SINCRONIZAR_ITENS_SINCRONIZACOES_SincronizarId",
                        column: x => x.SincronizarId,
                        principalTable: "SINCRONIZACOES",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SINCRONIZAR_ITENS_SincronizarId",
                table: "SINCRONIZAR_ITENS",
                column: "SincronizarId");

            migrationBuilder.Sql($"INSERT INTO CONFIGURACOES (Dia, HoraCron) VALUES (1, '16:30')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AZURE");

            migrationBuilder.DropTable(
                name: "CONFIGURACOES");

            migrationBuilder.DropTable(
                name: "CONTAS");

            migrationBuilder.DropTable(
                name: "SINCRONIZAR_ITENS");

            migrationBuilder.DropTable(
                name: "SINCRONIZACOES");
        }
    }
}
