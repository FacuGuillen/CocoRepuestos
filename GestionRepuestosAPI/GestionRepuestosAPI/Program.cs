using Microsoft.EntityFrameworkCore;
using GestionRepuestosAPI.Data;
using System.Text.Json.Serialization;

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

// 3. CONFIGURACIÓN DE CONTROLADORES Y JSON (Aquí está el cambio)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Esto ayuda a que el JSON mantenga la precisión de los números decimales
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
        // Evita ciclos de referencia si en el futuro agregás relaciones complejas
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// FORZAMOS SWAGGER
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CocoRepuestos API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("PermitirTodo");

app.UseAuthorization();
app.MapControllers();

app.Run();