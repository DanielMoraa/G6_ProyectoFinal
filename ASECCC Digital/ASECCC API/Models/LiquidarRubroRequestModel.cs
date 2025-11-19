namespace ASECCC_API.Models
{
    public class LiquidarRubroRequestModel
    {
        public int UsuarioId { get; set; }
        public string TipoRubro { get; set; } = string.Empty;
        public int IdRubro { get; set; }
    }
}