using Microsoft.EntityFrameworkCore;
using GestionRepuestosAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de la Base de Datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CONFIGURACIÓN DE CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// FORZAMOS SWAGGER (Sin el IF de IsDevelopment)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CocoRepuestos API V1");
    c.RoutePrefix = "swagger"; // Para entrar directo con /swagger
});

app.UseCors("PermitirTodo");

// Comentamos la redirección HTTPS para evitar errores en localhost:5000
// app.UseHttpsRedirection(); 

app.UseAuthorization();
app.MapControllers();

app.Run();