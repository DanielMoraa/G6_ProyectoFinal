namespace ASECCC_API.Models
{
    public class AporteResponseModel
    {
        public int AporteId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
        public DateTime? FechaRegistro { get; set; }
    }

    public class AporteAdminResponseModel : AporteResponseModel
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
    }

    public class CrearAporteRequestModel
    {
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
