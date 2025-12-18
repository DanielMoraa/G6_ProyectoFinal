namespace ASECCC_API.Models
{
    public class NotificacionResponseModel
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime? FechaEnvio { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class MarcarLeidaRequestModel
    {
        public int NotificacionId { get; set; }
    }

    public class MarcarTodasLeidasRequestModel
    {
        public int UsuarioId { get; set; }
    }

    public class NotificacionAdminResumenResponseModel
    {
        public string Titulo { get; set; } = string.Empty;
        public string EnviadaA { get; set; } = string.Empty;
        public DateTime? FechaEnvio { get; set; }
        public int Cantidad { get; set; }
    }

    public class CrearNotificacionMasivaRequestModel
    {
        public string Destino { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Tipo { get; set; } = "General";
    }
}
