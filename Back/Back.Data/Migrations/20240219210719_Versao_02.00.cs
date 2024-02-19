using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0200 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SINCRONIZACOES",
                columns: table => new
                {
                    Id = table.Column<int>(type: "Integer", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DataInicio = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DataFim = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(type: "Integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SINCRONIZACOES", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SINCRONIZACOES");
        }
    }
}
