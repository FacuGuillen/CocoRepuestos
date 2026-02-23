using GestionRepuestosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionRepuestosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<RepuestosStock> RepuestosStock { get; set; } = null!;
        public DbSet<RepuestosVenta> RepuestosVenta { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<CuentaCorriente> CuentasCorrientes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relaciones
            modelBuilder.Entity<RepuestosVenta>()
                .HasOne(v => v.RepuestoStock).WithMany().HasForeignKey(v => v.RepuestoStockId);

            modelBuilder.Entity<RepuestosVenta>()
                .HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId);

            modelBuilder.Entity<CuentaCorriente>()
                .HasOne(cc => cc.Cliente).WithMany().HasForeignKey(cc => cc.ClienteId);

            // PRECISIÓN: Aquí es donde evitamos que 0.055 se vuelva 0.06
            modelBuilder.Entity<RepuestosStock>().Property(r => r.PesoKg).HasPrecision(18, 4);
            modelBuilder.Entity<RepuestosStock>().Property(r => r.CostoUSD).HasPrecision(18, 2);
            modelBuilder.Entity<RepuestosVenta>().Property(v => v.TasaCambio).HasPrecision(18, 4);
            modelBuilder.Entity<RepuestosVenta>().Property(v => v.PrecioVentaUnitarioEnARS).HasPrecision(18, 2);
            modelBuilder.Entity<CuentaCorriente>().Property(c => c.Debe).HasPrecision(18, 2);
            modelBuilder.Entity<CuentaCorriente>().Property(c => c.Haber).HasPrecision(18, 2);
            modelBuilder.Entity<CuentaCorriente>().Property(c => c.Saldo).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}