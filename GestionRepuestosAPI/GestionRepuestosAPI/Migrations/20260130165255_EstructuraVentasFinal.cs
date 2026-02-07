using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionRepuestosAPI.Migrations
{
    /// <inheritdoc />
    public partial class EstructuraVentasFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrigenVenta",
                table: "RepuestosVenta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrigenVenta",
                table: "RepuestosVenta");
        }
    }
}
