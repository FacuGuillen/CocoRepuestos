using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionRepuestosAPI.Migrations
{
    /// <inheritdoc />
    public partial class BaseLimpiaNueva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

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
                    CostoUSD = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestosStock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasCorrientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Debe = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Haber = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasCorrientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasCorrientes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepuestosVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaVenta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TasaCambio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PrecioVentaUnitarioEnARS = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RepuestoStockId = table.Column<int>(type: "int", nullable: false),
                    cantidadVendida = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    OrigenVenta = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepuestosVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepuestosVenta_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepuestosVenta_RepuestosStock_RepuestoStockId",
                        column: x => x.RepuestoStockId,
                        principalTable: "RepuestosStock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasCorrientes_ClienteId",
                table: "CuentasCorrientes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestosVenta_ClienteId",
                table: "RepuestosVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_RepuestosVenta_RepuestoStockId",
                table: "RepuestosVenta",
                column: "RepuestoStockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuentasCorrientes");

            migrationBuilder.DropTable(
                name: "RepuestosVenta");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "RepuestosStock");
        }
    }
}
