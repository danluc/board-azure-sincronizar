using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    Token = table.Column<string>(maxLength: 255, nullable: false),
                    NomeUsuario = table.Column<string>(maxLength: 255, nullable: true),
                    UrlCorporacao = table.Column<string>(maxLength: 255, nullable: false),
                    ProjetoNome = table.Column<string>(maxLength: 255, nullable: true),
                    ProjetoId = table.Column<string>(maxLength: 255, nullable: true),
                    TimeNome = table.Column<string>(maxLength: 255, nullable: true),
                    TimeId = table.Column<string>(maxLength: 255, nullable: true),
                    AreaPath = table.Column<string>(nullable: true),
                    Sprint = table.Column<string>(maxLength: 255, nullable: true),
                    Principal = table.Column<bool>(type: "Bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTAS", x => x.Id);
                });

            migrationBuilder.Sql($"INSERT INTO CONFIGURACOES (Dia, HoraCron) VALUES (1, '11:30')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONFIGURACOES");

            migrationBuilder.DropTable(
                name: "CONTAS");
        }
    }
}
