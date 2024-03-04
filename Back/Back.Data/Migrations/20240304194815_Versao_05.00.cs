using Microsoft.EntityFrameworkCore.Migrations;

namespace Back.Data.Migrations
{
    public partial class Versao_0500 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SINCRONIZAR_ITENS");
        }
    }
}
