namespace ASECCC_API.Models
{
    public class CrearAhorroRequest
    {
        public int UsuarioId { get; set; }
        public int TipoAhorroId { get; set; }
        public decimal MontoInicial { get; set; }
        public int? Plazo { get; set; }
    }
}
