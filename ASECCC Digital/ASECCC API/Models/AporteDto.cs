namespace ASECCC_API.Models
{
    public class AporteDto
    {
        public int AporteId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string? NombreAsociado { get; set; }
    }
}
