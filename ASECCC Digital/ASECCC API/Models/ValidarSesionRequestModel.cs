using System.ComponentModel.DataAnnotations;

namespace ASECCC_API.Models
{
    public class ValidarSesionRequestModel
    {
        [Required]
        public string Identificacion { get; set; } = string.Empty;
        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }
}
