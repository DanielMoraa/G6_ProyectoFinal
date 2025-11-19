using System.ComponentModel.DataAnnotations;

namespace ASECCC_API.Models
{
    public class SeguridadRequestModel
    {
        [Required]
        public int UsuarioId { get; set; }
        [Required]
        public string Contrasena { get; set; } = string.Empty;
    }
}
