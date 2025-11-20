namespace ASECCC_Digital.Models
{
    public class SolicitudPrestamoViewModel
    {
        public SolicitudesAgrupadas Solicitudes { get; set; } = new();
    }

    public class SolicitudesAgrupadas
    {
        public List<SolicitudPrestamoDetalleModel> Pendientes { get; set; } = new();
        public List<SolicitudPrestamoDetalleModel> EnRevision { get; set; } = new();
        public List<SolicitudPrestamoDetalleModel> Aprobadas { get; set; } = new();
        public List<SolicitudPrestamoDetalleModel> Rechazadas { get; set; } = new();
    }

    public class SolicitudPrestamoDetalleModel
    {
        public int SolicitudPrestamoId { get; set; }
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal MontoSolicitud { get; set; }
        public string EstadoSolicitud { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
        public int PlazoMeses { get; set; }
        public UsuarioBasicoModel? Usuario { get; set; }
    }

    public class UsuarioBasicoModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
    }
}
