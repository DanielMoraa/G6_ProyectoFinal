namespace ASECCC_API.Models
{
    public class DatosUsuarioResponseModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string CorreoElectronico { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string EstadoAfiliacion { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public int PerfilId { get; set; }
        public bool Estado { get; set; }
        public string NombrePerfil { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
