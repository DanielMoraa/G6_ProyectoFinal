namespace ASECCC_API.Models
{
    public class AhorroDto
    {
        public int AhorroId { get; set; }
        public int UsuarioId { get; set; }
        public int TipoAhorroId { get; set; }
        public string TipoAhorro { get; set; } = string.Empty;
        public decimal MontoInicial { get; set; }
        public decimal MontoActual { get; set; }
        public DateTime? FechaInicio { get; set; }
        public int? Plazo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? NombreAsociado { get; set; }
    }
}
