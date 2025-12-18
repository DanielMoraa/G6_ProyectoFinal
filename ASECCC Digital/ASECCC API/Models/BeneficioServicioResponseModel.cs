namespace ASECCC_API.Models
{
    public class BeneficioServicioResponseModel
    {
        public int BeneficioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? Requisitos { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaRegistro { get; set; }
    }
}