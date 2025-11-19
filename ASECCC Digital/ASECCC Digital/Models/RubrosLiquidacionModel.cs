namespace ASECCC_Digital.Models
{
    public class RubrosLiquidacionModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public List<RubroLiquidacionModel> Rubros { get; set; } = new List<RubroLiquidacionModel>();
    }
}
