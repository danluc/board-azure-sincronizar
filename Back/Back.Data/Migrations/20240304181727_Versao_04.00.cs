using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0400 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CONFIGURACOES",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Porta",
                table: "CONFIGURACOES",
                type: "Int",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "SMTP",
                table: "CONFIGURACOES",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "CONFIGURACOES",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CONFIGURACOES");

            migrationBuilder.DropColumn(
                name: "Porta",
                table: "CONFIGURACOES");

            migrationBuilder.DropColumn(
                name: "SMTP",
                table: "CONFIGURACOES");

            migrationBuilder.DropColumn(
                name: "Senha",
                table: "CONFIGURACOES");
        }
    }
}
