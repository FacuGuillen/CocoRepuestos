namespace GestionRepuestosAPI.Models
{
    public class RepuestosStock
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string CodigoOriginal { get; set; }
        public int CantidadEnStock { get; set; }
        public decimal CostoUSD { get; set; }
        public decimal PesoKg { get; set; }
    }
}
