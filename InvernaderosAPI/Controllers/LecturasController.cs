using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InvernaderosAPI.Data;
using InvernaderosAPI.DTOs;
using InvernaderosAPI.Models;

namespace InvernaderosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturasController : ControllerBase
    {
        private readonly AppDbContext _db;

        public LecturasController(AppDbContext db)
        {
            _db = db;
        }

        // ── POST /api/lecturas ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> RecibirLectura([FromBody] LecturaDto dto)
        {
            // 1. Crear la lectura
            var lectura = new Lectura
            {
                IdSensor = dto.IdSensor,
                IdInvernadero = dto.IdInvernadero,
                FechaHora = DateTime.Now,
                Temperatura = dto.Temperatura,
                Humedad = dto.Humedad,
                Luminosidad = dto.Luminosidad,
                CalidadAire = dto.CalidadAire,
                HumedadSuelo = dto.HumedadSuelo,
                EsAlerta = false
            };

            // 2. Obtener umbrales del invernadero
            var umbrales = await _db.ConfiguracionesUmbral
                .Where(u => u.IdInvernadero == dto.IdInvernadero)
                .ToListAsync();

            // 3. Verificar alertas automáticamente
            var alertas = new List<Alerta>();

            foreach (var umbral in umbrales)
            {
                decimal valor = umbral.Variable switch
                {
                    "Temperatura" => dto.Temperatura,
                    "Humedad" => dto.Humedad,
                    _ => -1
                };

                if (valor < 0) continue;

                if (valor < umbral.ValorMinimo || valor > umbral.ValorMaximo)
                {
                    lectura.EsAlerta = true;

                    // Tipos exactos que acepta el CHECK constraint de Somee
                    string tipo = umbral.Variable switch
                    {
                        "Temperatura" => valor < umbral.ValorMinimo ? "TEMP_BAJA" : "TEMP_ALTA",
                        "Humedad" => valor < umbral.ValorMinimo ? "HUM_BAJA" : "HUM_ALTA",
                        _ => "TEMP_ALTA"
                    };

                    alertas.Add(new Alerta
                    {
                        IdInvernadero = dto.IdInvernadero,
                        TipoAlerta = tipo,
                        Mensaje = $"{umbral.Variable} fuera de rango: {valor} " +
                                         $"(rango: {umbral.ValorMinimo} - {umbral.ValorMaximo})",
                        ValorDetectado = valor,
                        FechaHora = DateTime.Now,
                        Resuelta = false
                    });
                }
            }

            // 4. Guardar lectura
            _db.Lecturas.Add(lectura);
            await _db.SaveChangesAsync();

            // 5. Guardar alertas con el IdLectura ya generado
            foreach (var alerta in alertas)
            {
                alerta.IdLectura = lectura.IdLectura;
                _db.Alertas.Add(alerta);
            }

            if (alertas.Any())
                await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Lectura guardada correctamente",
                idLectura = lectura.IdLectura,
                esAlerta = lectura.EsAlerta,
                alertas = alertas.Count
            });
        }

        // ── GET /api/lecturas/{idInvernadero} ─────────────────
        [HttpGet("{idInvernadero}")]
        [Authorize]
        public async Task<IActionResult> ObtenerLecturas(int idInvernadero)
        {
            var lecturas = await _db.Lecturas
                .Include(l => l.Invernadero)
                .Where(l => l.IdInvernadero == idInvernadero)
                .OrderByDescending(l => l.FechaHora)
                .Take(50)
                .Select(l => new LecturaResponseDto
                {
                    IdLectura = l.IdLectura,
                    FechaHora = l.FechaHora,
                    Temperatura = l.Temperatura,
                    Humedad = l.Humedad,
                    Luminosidad = l.Luminosidad,
                    CalidadAire = l.CalidadAire,
                    HumedadSuelo = l.HumedadSuelo,
                    EsAlerta = l.EsAlerta,
                    NombreInvernadero = l.Invernadero!.Nombre
                })
                .ToListAsync();

            return Ok(lecturas);
        }

        // ── GET /api/lecturas/{idInvernadero}/ultima ──────────
        [HttpGet("{idInvernadero}/ultima")]
        [Authorize]
        public async Task<IActionResult> UltimaLectura(int idInvernadero)
        {
            var lectura = await _db.Lecturas
                .Include(l => l.Invernadero)
                .Where(l => l.IdInvernadero == idInvernadero)
                .OrderByDescending(l => l.FechaHora)
                .Select(l => new LecturaResponseDto
                {
                    IdLectura = l.IdLectura,
                    FechaHora = l.FechaHora,
                    Temperatura = l.Temperatura,
                    Humedad = l.Humedad,
                    Luminosidad = l.Luminosidad,
                    CalidadAire = l.CalidadAire,
                    HumedadSuelo = l.HumedadSuelo,
                    EsAlerta = l.EsAlerta,
                    NombreInvernadero = l.Invernadero!.Nombre
                })
                .FirstOrDefaultAsync();

            if (lectura == null)
                return NotFound(new { mensaje = "No hay lecturas registradas" });

            return Ok(lectura);
        }
    }
}