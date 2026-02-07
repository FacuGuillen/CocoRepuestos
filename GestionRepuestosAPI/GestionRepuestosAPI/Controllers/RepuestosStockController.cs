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
    public class RepuestosStockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RepuestosStockController(AppDbContext context)
        {
            _context = context;
        }

        private bool RepuestosStockExists(int id)
        {
            return _context.RepuestosStock.Any(e => e.Id == id);
        }

        // GET: RepuestosStocks
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lista = await _context.RepuestosStock.ToListAsync();
            return Ok(lista); // <--- El 'Ok' convierte la lista en el tipo de dato que espera C#
        }

        // GET: api/RepuestosStock/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RepuestosStock>> GetRepuestos(int id)
        {
            // Buscamos el repuesto por ID (ej. una pieza de Peugeot o Nissan)
            var repuestosStock = await _context.RepuestosStock.FindAsync(id);

            if (repuestosStock == null)
            {
                // Si el repuesto no existe en tu inventario, devolvemos 404
                return NotFound();
            }

            // Retornamos el objeto JSON con la descripción, marca y costo en USD
            return repuestosStock;
        }


        // POST: api/RepuestosStock
        [HttpPost]
        public async Task<ActionResult<RepuestosStock>> PostRepuesto(RepuestosStock repuesto)
        {
            // Validamos que los datos (Nombre, Cantidad, Precio) sean correctos
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Agregamos el repuesto al contexto de la base de datos
            _context.RepuestosStock.Add(repuesto);
            await _context.SaveChangesAsync();

            // Devolvemos un código 201 y la ruta para consultar este repuesto
            return CreatedAtAction("GetRepuestos", new { id = repuesto.Id }, repuesto);
        }


        // PUT: api/RepuestosStock/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRepuesto(int id, RepuestosStock repuestosStock)
        {
            if (id != repuestosStock.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(repuestosStock).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RepuestosStockExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // Éxito: 204 No Content
        }

        // DELETE: api/RepuestosStock/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRepuesto(int id)
        {
            // Buscamos el repuesto (ej. una pieza de Peugeot o Nissan)
            var repuesto = await _context.RepuestosStock.FindAsync(id);

            if (repuesto == null)
            {
                // Si no existe, devolvemos 404
                return NotFound();
            }

            // Lo eliminamos de la tabla de stock
            _context.RepuestosStock.Remove(repuesto);
            await _context.SaveChangesAsync();

            // Retornamos 204 No Content para confirmar el éxito en la API
            return NoContent();
        }
    }
}
