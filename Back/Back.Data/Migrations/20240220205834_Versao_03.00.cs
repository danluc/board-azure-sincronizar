using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0300 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cliente",
                table: "CONFIGURACOES",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cliente",
                table: "CONFIGURACOES");
        }
    }
}
