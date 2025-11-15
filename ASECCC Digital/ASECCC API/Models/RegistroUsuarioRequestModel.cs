using System.ComponentModel.DataAnnotations;

namespace ASECCC_API.Models
{
    public class RegistroUsuarioRequestModel
    {

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;
        [Required]
        public string CorreoElectronico { get; set; } = string.Empty;
        [Required]
        public string Contrasena { get; set; } = string.Empty;
        [Required]
        public string TipoIdentificacion { get; set; } = string.Empty;
        [Required]
        public string Identificacion { get; set; } = string.Empty;
        [Required]
        public DateTime FechaNacimiento { get; set; }
        [Required]
        public string Telefono { get; set; } = string.Empty;
        [Required]
        public string Direccion { get; set; } = string.Empty;
    }
}
