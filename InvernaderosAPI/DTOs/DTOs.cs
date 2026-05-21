namespace InvernaderosAPI.DTOs
{
    // ── LOGIN ─────────────────────────────────────────────────
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int? IdInvernadero { get; set; }
    }

    // ── LECTURA (lo que envía el ESP32) ───────────────────────
    public class LecturaDto
    {
        public int IdSensor { get; set; }
        public int IdInvernadero { get; set; }
        public decimal Temperatura { get; set; }
        public decimal Humedad { get; set; }
        public decimal? Luminosidad { get; set; }
        public decimal? CalidadAire { get; set; }
        public decimal? HumedadSuelo { get; set; }
    }

    public class LecturaResponseDto
    {
        public long IdLectura { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Temperatura { get; set; }
        public decimal Humedad { get; set; }
        public decimal? Luminosidad { get; set; }
        public decimal? CalidadAire { get; set; }
        public decimal? HumedadSuelo { get; set; }
        public bool EsAlerta { get; set; }
        public string NombreInvernadero { get; set; } = string.Empty;
    }

    // ── DASHBOARD ─────────────────────────────────────────────
    public class DashboardDto
    {
        public decimal UltimaTemperatura { get; set; }
        public decimal UltimaHumedad { get; set; }
        public decimal? UltimaLuminosidad { get; set; }
        public decimal? UltimaCalidadAire { get; set; }
        public decimal? UltimaHumedadSuelo { get; set; }
        public DateTime UltimaLectura { get; set; }
        public int TotalAlertasHoy { get; set; }
        public int TotalLecturasHoy { get; set; }
        public bool SistemaActivo { get; set; }
    }

    // ── ALERTA ────────────────────────────────────────────────
    public class AlertaResponseDto
    {
        public int IdAlerta { get; set; }
        public string TipoAlerta { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public decimal ValorDetectado { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Resuelta { get; set; }
        public string NombreInvernadero { get; set; } = string.Empty;
    }

    // ── UMBRAL ────────────────────────────────────────────────
    public class UmbralDto
    {
        public string Variable { get; set; } = string.Empty;
        public decimal ValorMinimo { get; set; }
        public decimal ValorMaximo { get; set; }
    }
}