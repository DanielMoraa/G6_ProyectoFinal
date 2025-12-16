namespace ASECCC_Digital.Models
{
    public class BeneficioServicioModel
    {
        public int BeneficioId { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? Requisitos { get; set; }
        public string Estado { get; set; } = "";
        public DateTime? FechaRegistro { get; set; }
    }
}
