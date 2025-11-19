namespace ASECCC_API.Models
{
    public class LiquidarRubroResponseModel
    {
        public int FilasAfectadas { get; set; }
        public bool Exito => FilasAfectadas > 0;
        public string Mensaje => Exito ? "Rubro liquidado correctamente" : "No se pudo liquidar el rubro";
    }
}
