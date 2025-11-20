namespace ASECCC_Digital.Models
{
    public class SolicitudPrestamoModel
    {
        public int UsuarioId { get; set; }
        public string EstadoCivil { get; set; } = string.Empty;
        public bool PagaAlquiler { get; set; }
        public decimal? MontoAlquiler { get; set; }
        public string? NombreAcreedor { get; set; }
        public decimal? TotalCredito { get; set; }
        public decimal? AbonoSemanal { get; set; }
        public decimal? SaldoCredito { get; set; }
        public string? NombreDeudor { get; set; }
        public decimal? TotalPrestamo { get; set; }
        public decimal? SaldoPrestamo { get; set; }
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal MontoSolicitud { get; set; }
        public decimal CuotaSemanalSolicitud { get; set; }
        public int PlazoMeses { get; set; }
        public string PropositoPrestamo { get; set; } = string.Empty;
    }

    public class PrestamoModel
    {
        public int PrestamoId { get; set; }
        public int UsuarioId { get; set; }
        public decimal? MontoAprobado { get; set; }
        public int Plazo { get; set; }
        public decimal? CuotaSemanal { get; set; }
        public string TipoPrestamo { get; set; } = string.Empty;
        public string EstadoPrestamo { get; set; } = string.Empty;
        public DateTime? FechaSolicitud { get; set; }
        public DateTime? FechaEstado { get; set; }
        public decimal? SaldoPendiente { get; set; }
        public string? Observaciones { get; set; }
    }

    public class PrestamoDetalleModel : PrestamoModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public List<PrestamoTransaccionModel> Transacciones { get; set; } = new();
    }

    public class PrestamoTransaccionModel
    {
        public int TransaccionPrestamoId { get; set; }
        public int PrestamoId { get; set; }
        public decimal MontoAbonado { get; set; }
        public DateTime? FechaPago { get; set; }
    }

    public class AbonoPrestamoModel
    {
        public int PrestamoId { get; set; }
        public decimal MontoAbonado { get; set; }
    }

    public class SolicitudPrestamoListaModel
    {
        public int SolicitudPrestamoId { get; set; }
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal MontoSolicitud { get; set; }
        public string EstadoSolicitud { get; set; } = string.Empty;
        public DateTime? FechaSolicitud { get; set; }
        public int PlazoMeses { get; set; }
    }

    public class SolicitudPrestamoCompletaModel
    {
        public int SolicitudPrestamoId { get; set; }
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string EstadoCivil { get; set; } = string.Empty;
        public string PagaAlquiler { get; set; } = string.Empty;
        public decimal? MontoAlquiler { get; set; }
        public string? NombreAcreedor { get; set; }
        public decimal? TotalCredito { get; set; }
        public decimal? AbonoSemanal { get; set; }
        public decimal? SaldoCredito { get; set; }
        public string? NombreDeudor { get; set; }
        public decimal? TotalPrestamo { get; set; }
        public decimal? SaldoPrestamo { get; set; }
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal MontoSolicitud { get; set; }
        public string EstadoSolicitud { get; set; } = string.Empty;
        public decimal CuotaSemanalSolicitud { get; set; }
        public int PlazoMeses { get; set; }
        public string PropositoPrestamo { get; set; } = string.Empty;
        public DateTime FechaSolicitud { get; set; }
    }
    
}