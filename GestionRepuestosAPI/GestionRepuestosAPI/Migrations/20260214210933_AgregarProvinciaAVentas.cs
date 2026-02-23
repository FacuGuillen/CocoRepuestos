using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionRepuestosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarProvinciaAVentas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "RepuestosVenta",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "RepuestosVenta");
        }
    }
}
