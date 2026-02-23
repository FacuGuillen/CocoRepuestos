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
    public class CuentaCorrientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CuentaCorrientesController(AppDbContext context)
        {
            _context = context;
        }

        private bool CuentaCorrienteExists(int id)
        {
            return _context.CuentasCorrientes.Any(e => e.Id == id);
        }

        // GET: api/CuentaCorrientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuentaCorriente>>> GetCuentasCorrientes()
        {
            // Usamos Include para que el resultado traiga los datos del Cliente (como su nombre)
            var cuentas = await _context.CuentasCorrientes
                .Include(c => c.Cliente)
                .ToListAsync();

            return Ok(cuentas);
        }

        // GET: api/CuentaCorrientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CuentaCorriente>> GetCuentaCorriente(int id)
        {
            // Buscamos el movimiento incluyendo los datos del cliente (ej. Dani)
            var cuentaCorriente = await _context.CuentasCorrientes
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cuentaCorriente == null)
            {
                return NotFound();
            }

            return cuentaCorriente;
        }

        [HttpGet("saldo/{nombreDelCliente}")]
        public async Task<ActionResult> GetSaldoCliente(string nombreDelCliente)
        {
            // 1. Buscamos al cliente primero para asegurar que existe
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Nombre.ToLower().Trim() == nombreDelCliente.ToLower().Trim());

            if (cliente == null)
            {
                return NotFound($"El cliente '{nombreDelCliente}' no existe en CocoRepuestos.");
            }

            // 2. Traemos sus movimientos usando el ID (que es infalible)
            var movimientos = await _context.CuentasCorrientes
                .Where(cc => cc.ClienteId == cliente.Id)
                .OrderBy(cc => cc.Fecha)
                .ToListAsync();

            // 3. Procesamos los datos para el JS
            var totalDebe = movimientos.Sum(cc => cc.Debe);
            var totalHaber = movimientos.Sum(cc => cc.Haber);
            var saldoAcumulado = 0m;

            var movimientosConSaldo = movimientos.Select(m => {
                saldoAcumulado += (m.Debe - m.Haber);
                return new
                {
                    m.Id,
                    m.Fecha,
                    m.Detalle,
                    m.Debe,
                    m.Haber,
                    SaldoAcumulado = saldoAcumulado
                };
            }).ToList();

            return Ok(new
            {
                ClienteId = cliente.Id,
                Cliente = cliente.Nombre,
                TotalDeudaOriginal = totalDebe,
                TotalPagadoHastaHoy = totalHaber,
                SaldoPendiente = totalDebe - totalHaber,
                Movimientos = movimientosConSaldo
            });
        }


        // POST: api/CuentaCorrientes
        [HttpPost]
        public async Task<ActionResult<CuentaCorriente>> PostCuentaCorriente(CuentaCorriente cuentaCorriente)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.CuentasCorrientes.Add(cuentaCorriente);
            await _context.SaveChangesAsync();

            // Recargamos el objeto para incluir los datos del cliente (ej. Dani) en la respuesta
            await _context.Entry(cuentaCorriente).Reference(c => c.Cliente).LoadAsync();

            return CreatedAtAction("GetCuentaCorriente", new { id = cuentaCorriente.Id }, cuentaCorriente);
        }

        [HttpPost("registrar-pago")]
        public async Task<ActionResult> RegistrarPagoCuentaCorriente(int clienteId, decimal monto, string detalleDelPago, DateTime? fecha)
        {
            // 1. Verificamos que el cliente exista
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null) return NotFound("El cliente no existe.");

            var pago = new CuentaCorriente
            {
                ClienteId = clienteId,
                Fecha = fecha ?? DateTime.Now,
                Detalle = $"Pago recibido: {detalleDelPago}",
                Debe = 0,
                Haber = monto
            };

            _context.CuentasCorrientes.Add(pago);
            await _context.SaveChangesAsync();

            var movimientosActualizados = await _context.CuentasCorrientes
                .Where(cc => cc.ClienteId == clienteId)
                .ToListAsync();

            var saldoRestante = movimientosActualizados.Sum(m => m.Debe) - movimientosActualizados.Sum(m => m.Haber);

            return Ok(new
            {
                Mensaje = "Pago registrado exitosamente.",
                SaldoRestante = saldoRestante,
                Movimientos = movimientosActualizados
            });

        }



        // PUT: api/CuentaCorrientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuentaCorriente(int id, CuentaCorriente cuentaCorriente)
        {
            if (id != cuentaCorriente.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(cuentaCorriente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuentaCorrienteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // DELETE: api/CuentaCorrientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuentaCorriente(int id)
        {
            // Buscamos el movimiento específico en la base de datos
            var cuentaCorriente = await _context.CuentasCorrientes.FindAsync(id);

            if (cuentaCorriente == null)
            {
                // Si el registro no existe, devolvemos un 404
                return NotFound();
            }

            // Lo eliminamos y guardamos los cambios en SQL Server
            _context.CuentasCorrientes.Remove(cuentaCorriente);
            await _context.SaveChangesAsync();

            // Retornamos 204 (No Content) para confirmar el éxito en Swagger
            return NoContent();
        }
    }
}
