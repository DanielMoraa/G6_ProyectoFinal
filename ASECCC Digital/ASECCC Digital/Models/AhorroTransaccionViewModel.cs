namespace ASECCC_Digital.Models
{
    public class AhorroTransaccionViewModel
    {
        public int TransaccionId { get; set; }
        public int AhorroId { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }
}
