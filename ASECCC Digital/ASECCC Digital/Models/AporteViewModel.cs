namespace ASECCC_Digital.Models
{
    public class AporteViewModel
    {
        public int AporteId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime? FechaRegistro { get; set; }
        public string NombreAsociado { get; set; } = string.Empty;
    }
}
