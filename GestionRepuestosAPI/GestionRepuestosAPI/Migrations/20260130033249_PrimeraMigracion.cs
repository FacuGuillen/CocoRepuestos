using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionRepuestosAPI.Migrations
{
    /// <inheritdoc />
    public partial class PrimeraMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepuestosStock",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CodigoOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadEnStock = table.Column<int>(type: "int", nullable: false),
                    CostoUSD = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestosStock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RepuestosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioVentaUnitarioEnARS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RepuestoStockId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepuestosVenta_RepuestosStock_RepuestoStockId",
                        column: x => x.RepuestoStockId,
                        principalTable: "RepuestosStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepuestosVenta_RepuestoStockId",
                table: "RepuestosVenta",
                column: "RepuestoStockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepuestosVenta");

            migrationBuilder.DropTable(
                name: "RepuestosStock");
        }
    }
}
