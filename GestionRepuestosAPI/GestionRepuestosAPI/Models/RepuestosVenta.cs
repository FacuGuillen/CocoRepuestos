using System;

namespace GestionRepuestosAPI.Models
{
    public class RepuestosVenta
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; } = DateTime.Now;
        public decimal TasaCambio { get; set; }
        public decimal PrecioVentaUnitarioEnARS { get; set; }
        public int RepuestoStockId { get; set; }
        public virtual RepuestosStock? RepuestoStock { get; set; } // Agregamos virtual por buena práctica

        public int cantidadVendida { get; set; } = 1;

        public decimal CostoUnitarioEnARS => (RepuestoStock?.CostoUSD ?? 0) * TasaCambio;
        public decimal GananciaPorProducto => PrecioVentaUnitarioEnARS - CostoUnitarioEnARS;
        public decimal GananciaTotalVenta => GananciaPorProducto * cantidadVendida;
        public decimal MontoTotalOperacion => PrecioVentaUnitarioEnARS * cantidadVendida;

        public int ClienteId { get; set; }
        public virtual Cliente? Cliente { get; set; } // Agregamos virtual
        public string OrigenVenta { get; set; } = string.Empty; // Evita nulos
        public string? Provincia { get; set; } // Agregale el ?
    }
}