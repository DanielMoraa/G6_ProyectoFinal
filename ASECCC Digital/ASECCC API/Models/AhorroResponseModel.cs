namespace ASECCC_API.Models
{
    public class AhorroResponseModel
    {
        public int AhorroId { get; set; }
        public int UsuarioId { get; set; }
        public int TipoAhorroId { get; set; }
        public string TipoAhorro { get; set; } = string.Empty;

        public decimal MontoInicial { get; set; }
        public decimal MontoActual { get; set; }

        public DateTime? FechaInicio { get; set; }
        public int? Plazo { get; set; }

        public string Estado { get; set; } = string.Empty;

        public string NombreAsociado { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
    }

    public class AhorroTransaccionResponseModel
    {
        public int TransaccionId { get; set; }
        public int AhorroId { get; set; }
        public DateTime? Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string? Descripcion { get; set; }
    }

    public class CrearAhorroRequestModel
    {
        public int UsuarioId { get; set; }
        public int TipoAhorroId { get; set; }
        public decimal MontoInicial { get; set; }
        public int? Plazo { get; set; }
    }

    public class CatalogoTipoAhorroResponseModel
    {
        public int TipoAhorroId { get; set; }
        public string TipoAhorro { get; set; } = "";
    }

}
