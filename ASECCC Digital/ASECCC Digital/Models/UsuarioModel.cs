using System.ComponentModel.DataAnnotations;

namespace ASECCC_Digital.Models
{
    public class UsuarioModel
    {
        [Key]
        [Required(ErrorMessage = "La identificación es obligatoria")]
        public int Identificacion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ingresar un correo válido")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "La contraseña no puede superar los 255 caracteres")]
        public string Contrasenna { get; set; } = string.Empty;
    }
}

