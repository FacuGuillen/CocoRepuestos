using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionRepuestosAPI.Data;
using GestionRepuestosAPI.Models;

namespace GestionRepuestosAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepuestosVentasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RepuestosVentasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: RepuestosVentas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RepuestosVenta>>> GetRepuestosVenta()
        {
            var venta = await _context.RepuestosVenta
                .Include(v => v.RepuestoStock)
                .Include(v => v.Cliente) // <-- AGREGÁ ESTA LÍNEA
                .ToListAsync();
            return Ok(venta);
        }

        // GET: api/RepuestosVentas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RepuestosVenta>> GetRepuestosVenta(int id)
        {
            // Buscamos la venta e incluimos el repuesto para saber qué se vendió
            var repuestosVenta = await _context.RepuestosVenta
                .Include(r => r.RepuestoStock)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (repuestosVenta == null)
            {
                return NotFound();
            }

            return repuestosVenta;
        }

        [HttpGet("reporte-ganancias/{mes}/{anio}")]
        public async Task<ActionResult> GetReporteGanancias(int mes, int anio)
        {
            // 1. Traemos las ventas incluyendo el stock para los cálculos
            var ventas = await _context.RepuestosVenta
                .Include(v => v.RepuestoStock)
                .Where(v => v.FechaVenta.Month == mes && v.FechaVenta.Year == anio)
                .ToListAsync();

            if (!ventas.Any())
            {
                return NotFound($"No hay ventas registradas para el periodo {mes}/{anio}.");
            }

            // 2. Usamos las propiedades calculadas que creamos en el modelo (las que contemplan cantidad)
            var totalGanancia = ventas.Sum(v => v.GananciaTotalVenta);
            var totalFacturadoARS = ventas.Sum(v => v.MontoTotalOperacion);

            // 3. Desglose opcional por origen (Mercado Libre vs Cuenta Corriente)
            var gananciaML = ventas.Where(v => v.OrigenVenta == "Mercado Libre").Sum(v => v.GananciaTotalVenta);
            var gananciaCC = ventas.Where(v => v.OrigenVenta == "Cuenta Corriente").Sum(v => v.GananciaTotalVenta);

            // 4. Devolvemos el reporte completo
            return Ok(new
            {
                Periodo = $"{mes}/{anio}",
                TotalFacturado = totalFacturadoARS,
                GananciaNetaTotal = totalGanancia,
                DetalleGanancias = new
                {
                    MercadoLibre = gananciaML,
                    CuentaCorriente = gananciaCC
                },
                CantidadOperaciones = ventas.Count,
                TotalUnidadesVendidas = ventas.Sum(v => v.cantidadVendida)
            });
        }

        [HttpGet("reporte-ganancias-cc/{mes}/{anio}/{cliente}")]
        public async Task<ActionResult> GetReporteGananciaCuentaCorriente(int mes, int anio, string cliente)
        {
            // 1. Filtramos las ventas del mes/año para ese cliente específico
            var ventas = await _context.RepuestosVenta
                .Include(v => v.RepuestoStock)
                .Include(v => v.Cliente)
                .Where(v => v.FechaVenta.Month == mes &&
                            v.FechaVenta.Year == anio &&
                            v.OrigenVenta == "Cuenta Corriente" &&
                            v.Cliente.Nombre.ToLower().Contains(cliente.ToLower())) // Filtro de nombre
                .ToListAsync();

            if (!ventas.Any())
            {
                return NotFound($"No hay ventas en CC para el periodo {mes}/{anio} y cliente {cliente}.");
            }

            // 2. Sumamos usando la propiedad de Ganancia Total (Cantidad * Ganancia Unitaria)
            // Ya no filtramos por OrigenVenta aquí porque ya lo hicimos arriba en el Where
            var totalGananciaCC = ventas.Sum(v => v.GananciaTotalVenta);
            var totalFacturadoCC = ventas.Sum(v => v.MontoTotalOperacion);

            return Ok(new
            {
                Periodo = $"{mes}/{anio}",
                ClienteIdentificado = ventas.First().Cliente?.Nombre, // Mostramos el nombre real del cliente encontrado
                VentasTotalesARS = totalFacturadoCC,
                GananciaTotalCuentaCorriente = totalGananciaCC,
                CantidadOperaciones = ventas.Count,
                TotalUnidadesVendidas = ventas.Sum(v => v.cantidadVendida)
            });
        }

        [HttpGet("reporte-convenio-multilateral/{anio}/{mes}")]
        public async Task<ActionResult> GetReporteConvenio(int anio, int mes)
        {
            var datosConvenio = await _context.RepuestosVenta
                .Where(v => v.FechaVenta.Year == anio && v.FechaVenta.Month == mes)
                .GroupBy(v => v.Provincia)
                .Select(g => new
                {
                    Jurisdiccion = g.Key ?? "Sin Definir",
                    TotalIngresos = g.Sum(v => v.MontoTotalOperacion), // Este es el monto imponible
                    CantidadOperaciones = g.Count()
                })
                .OrderBy(res => res.Jurisdiccion)
                .ToListAsync();

            if (!datosConvenio.Any()) return NotFound("No hay ventas registradas en este periodo.");

            return Ok(new
            {
                Anio = anio,
                Mes = mes,
                DetallePorJurisdiccion = datosConvenio,
                TotalGeneralMes = datosConvenio.Sum(d => d.TotalIngresos)
            });
        }


        [HttpPost]
        public async Task<ActionResult<RepuestosVenta>> PostRepuestosVenta(RepuestosVenta repuestosVenta)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Buscamos el repuesto para validar stock
            var repuesto = await _context.RepuestosStock.FindAsync(repuestosVenta.RepuestoStockId);
            if (repuesto == null) return NotFound("El repuesto no existe.");

            // 2. Validamos disponibilidad
            if (repuesto.CantidadEnStock < repuestosVenta.cantidadVendida)
            {
                return BadRequest($"Stock insuficiente. Disponible: {repuesto.CantidadEnStock}");
            }

            // 3. Descontamos las unidades del inventario
            repuesto.CantidadEnStock -= repuestosVenta.cantidadVendida;
            _context.Entry(repuesto).State = EntityState.Modified;

            // 4. Lógica de Cuenta Corriente (SACAMOS EL IF)
            // Ahora toda venta genera un movimiento en la cuenta del cliente seleccionado
            var movimientoCC = new CuentaCorriente
            {
                ClienteId = repuestosVenta.ClienteId,
                Fecha = repuestosVenta.FechaVenta,
                Detalle = $"Venta: {repuesto.Descripcion} (Cant: {repuestosVenta.cantidadVendida})",
                // Usamos el monto total que viene del JS
                Debe = repuestosVenta.MontoTotalOperacion,
                Haber = 0
            };
            _context.CuentasCorrientes.Add(movimientoCC);

            // 5. Guardamos la venta
            _context.RepuestosVenta.Add(repuestosVenta);

            // El SaveChangesAsync guarda las TRES cosas: el stock, el movimiento de guita y la venta
            await _context.SaveChangesAsync();

            // 6. Carga de referencias para la respuesta
            await _context.Entry(repuestosVenta).Reference(v => v.RepuestoStock).LoadAsync();
            await _context.Entry(repuestosVenta).Reference(v => v.Cliente).LoadAsync();

            return CreatedAtAction("GetRepuestosVenta", new { id = repuestosVenta.Id }, repuestosVenta);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutRepuestosVenta(int id, RepuestosVenta ventaEditada)
        {
            if (id != ventaEditada.Id) return BadRequest();

            // 1. Buscamos la venta original para comparar cantidades (sin seguimiento para evitar conflictos)
            var ventaOriginal = await _context.RepuestosVenta
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (ventaOriginal == null) return NotFound();

            // 2. Buscamos el repuesto para actualizar su stock
            var repuesto = await _context.RepuestosStock.FindAsync(ventaEditada.RepuestoStockId);
            if (repuesto == null) return NotFound("Repuesto no encontrado.");

            // 3. Calculamos la diferencia de cantidad
            // Si editás de 1 a 2, la diferencia es +1 (hay que restar 1 al stock)
            // Si editás de 3 a 1, la diferencia es -2 (hay que devolver 2 al stock)
            int diferencia = ventaEditada.cantidadVendida - ventaOriginal.cantidadVendida;

            // 4. Validamos stock si hay un incremento en la venta
            if (diferencia > 0 && repuesto.CantidadEnStock < diferencia)
            {
                return BadRequest($"No hay suficiente stock. Disponible: {repuesto.CantidadEnStock}");
            }

            // 5. Actualizamos el stock automáticamente
            repuesto.CantidadEnStock -= diferencia;
            _context.Entry(repuesto).State = EntityState.Modified;

            // 6. Actualizamos el movimiento en Cuenta Corriente si corresponde
            if (ventaEditada.OrigenVenta == "Cuenta Corriente")
            {
                // Buscamos el movimiento relacionado por fecha y cliente
                var movimientoCC = await _context.CuentasCorrientes
                    .FirstOrDefaultAsync(c => c.Fecha == ventaOriginal.FechaVenta && c.ClienteId == ventaOriginal.ClienteId);

                if (movimientoCC != null)
                {
                    // Usamos la propiedad que calcula el Monto Total (Precio * Cantidad)
                    movimientoCC.Debe = ventaEditada.MontoTotalOperacion;
                    movimientoCC.Detalle = $"Venta Editada: {repuesto.Descripcion} (Cant: {ventaEditada.cantidadVendida})";
                    _context.Entry(movimientoCC).State = EntityState.Modified;
                }
            }

            // 7. Guardamos la venta con los nuevos datos
            _context.Entry(ventaEditada).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RepuestosVentaExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/RepuestosVentas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepuestosVenta(int id)
        {
            // Buscamos el registro de la venta en la base de datos
            var repuestosVenta = await _context.RepuestosVenta.FindAsync(id);

            if (repuestosVenta == null)
            {
                // Si no existe (por ejemplo, si ya se borró), devolvemos 404
                return NotFound();
            }

            // Eliminamos el registro
            _context.RepuestosVenta.Remove(repuestosVenta);
            await _context.SaveChangesAsync();

            // En APIs, lo ideal es devolver un 204 (No Content) para confirmar el éxito
            return NoContent();
        }

        private bool RepuestosVentaExists(int id)
        {
            return _context.RepuestosVenta.Any(e => e.Id == id);
        }
    }
}
