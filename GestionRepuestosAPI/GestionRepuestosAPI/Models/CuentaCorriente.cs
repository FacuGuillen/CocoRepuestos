namespace GestionRepuestosAPI.Models
{
    public class CuentaCorriente
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; } = null!;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Detalle { get; set; } = string.Empty;
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }
        public decimal Saldo { get; set; }
    }
}
