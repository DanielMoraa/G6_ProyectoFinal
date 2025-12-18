namespace ASECCC_API.Models
{
    public class ObtenerRubrosLiquidacionRequestModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public List<RubroLiquidacionResponseModel> Rubros { get; set; } = new List<RubroLiquidacionResponseModel>();
    }
}