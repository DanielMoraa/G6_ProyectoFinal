namespace ASECCC_Digital.Models
{
    public class NotificacionViewModel
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime? FechaEnvio { get; set; }
        public string Estado { get; set; } = string.Empty;

        public bool Leido => string.Equals(Estado, "Leida", StringComparison.OrdinalIgnoreCase);
    }

    public class NotificacionAdminResumenViewModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string EnviadaA { get; set; } = string.Empty;
        public DateTime? FechaEnvio { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearNotificacionMasivaViewModel
    {
        public string Destino { get; set; } = "todos";
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Tipo { get; set; } = "General";
    }
}
