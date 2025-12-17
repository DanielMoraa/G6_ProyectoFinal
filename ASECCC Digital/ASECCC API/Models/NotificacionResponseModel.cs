namespace ASECCC_API.Models
{
    public class NotificacionResponseModel
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public bool Leido { get; set; }
    }

    public class MarcarLeidaRequestModel
    {
        public int NotificacionId { get; set; }
    }

    public class MarcarTodasLeidasRequestModel
    {
        public int UsuarioId { get; set; }
    }
}
