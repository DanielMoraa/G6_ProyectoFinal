namespace ASECCC_API.Models
{
    public class CrearAporteRequest
    {
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
