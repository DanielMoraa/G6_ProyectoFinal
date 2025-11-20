namespace ASECCC_API.Models
{
    public class AhorroTransaccionDto
    {
        public int TransaccionId { get; set; }
        public int AhorroId { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }
}
