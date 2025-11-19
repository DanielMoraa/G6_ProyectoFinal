namespace ASECCC_Digital.Models
{
    public class RubroLiquidacionModel
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public int IdRubro { get; set; }
    }
}
