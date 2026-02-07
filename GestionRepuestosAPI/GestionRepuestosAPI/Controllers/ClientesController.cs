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
    public class ClientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Clientes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Clientes.ToListAsync();
            return Ok(lista); // <--- El 'Ok' convierte la lista en el tipo de dato que espera C#
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return cliente;
        }


        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            // Verificamos que los datos sean válidos (ej: que el nombre no esté vacío)
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            // Devuelve el código 201 y la ubicación del nuevo recurso
            return CreatedAtAction("GetCliente", new { id = cliente.Id }, cliente);
        }


        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            // Verificamos que el ID de la URL coincida con el ID del objeto enviado
            if (id != cliente.Id)
            {
                return BadRequest();
            }

            // Le decimos a Entity Framework que este cliente ya existe y fue modificado
            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Si hay un error de concurrencia, verificamos si el cliente aún existe
                if (!ClienteExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // En las APIs, una edición exitosa suele devolver 204 No Content
            return NoContent();
        }


        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            // Buscamos al cliente por su ID
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                // Si no existe, avisamos con un 404
                return NotFound();
            }

            // Lo removemos del contexto y guardamos los cambios en SQL
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            // Retornamos 204 (No Content) para confirmar que se borró con éxito
            return NoContent();
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }
}
