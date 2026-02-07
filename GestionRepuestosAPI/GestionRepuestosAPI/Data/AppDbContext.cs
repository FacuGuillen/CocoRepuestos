using GestionRepuestosAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GestionRepuestosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Estas son tus "tablas" en la base de datos (tus hojas de Excel)
        public DbSet<RepuestosStock> RepuestosStock { get; set; } = null!;
        public DbSet<RepuestosVenta> RepuestosVenta { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<CuentaCorriente> CuentasCorrientes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // La relación que ya tenías de Venta con Stock
            modelBuilder.Entity<RepuestosVenta>()
                .HasOne(v => v.RepuestoStock)
                .WithMany()
                .HasForeignKey(v => v.RepuestoStockId);

            // NUEVA: Relación de Venta con Cliente
            modelBuilder.Entity<RepuestosVenta>()
                .HasOne(v => v.Cliente)
                .WithMany()
                .HasForeignKey(v => v.ClienteId);

            // NUEVA: Relación de Cuenta Corriente con Cliente
            modelBuilder.Entity<CuentaCorriente>()
                .HasOne(cc => cc.Cliente)
                .WithMany()
                .HasForeignKey(cc => cc.ClienteId);

            base.OnModelCreating(modelBuilder);
        }
    }
}