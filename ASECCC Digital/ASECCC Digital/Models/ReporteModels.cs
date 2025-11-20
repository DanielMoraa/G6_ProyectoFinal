namespace ASECCC_Digital.Models
{
    public class ResumenFinancieroModel
    {
        public decimal TotalPrestamos { get; set; }
        public int CantidadPrestamos { get; set; }
        public decimal TotalAhorros { get; set; }
        public int CantidadAhorros { get; set; }
        public decimal TotalAportes { get; set; }
        public int CantidadAportes { get; set; }
    }

    public class TransaccionReporteModel
    {
        public string TipoTransaccion { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }

    public class EstadoCuentaModel
    {
        public UsuarioReporteModel? Usuario { get; set; }
        public List<PrestamoReporteModel> Prestamos { get; set; } = new();
        public List<AhorroReporteModel> Ahorros { get; set; } = new();
        public List<AporteReporteModel> Aportes { get; set; } = new();
    }

    public class UsuarioReporteModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
    }

    public class PrestamoReporteModel
    {
        public int PrestamoId { get; set; }
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal? MontoAprobado { get; set; }
        public decimal? SaldoPendiente { get; set; }
        public decimal? CuotaSemanal { get; set; }
        public string EstadoPrestamo { get; set; } = string.Empty;
    }

    public class AhorroReporteModel
    {
        public int AhorroId { get; set; }
        public string TipoAhorro { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? Plazo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class AporteReporteModel
    {
        public int AporteId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }

    public class PrestamoReporteDetalladoModel
    {
        public int PrestamoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string TipoPrestamo { get; set; } = string.Empty;
        public decimal? MontoAprobado { get; set; }
        public decimal? SaldoPendiente { get; set; }
        public decimal? CuotaSemanal { get; set; }
        public int Plazo { get; set; }
        public string EstadoPrestamo { get; set; } = string.Empty;
        public DateTime? FechaSolicitud { get; set; }
    }

    public class AhorroReporteDetalladoModel
    {
        public int AhorroId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string TipoAhorro { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? Plazo { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}