using System.ComponentModel.DataAnnotations;

namespace ASECCC_Digital.Models
{
    public class ContactoSoporteVm
    {
        [Required, StringLength(80)]
        public string Nombre { get; set; } = "";

        [Required, EmailAddress, StringLength(120)]
        public string Correo { get; set; } = "";

        [Required, StringLength(120)]
        public string Asunto { get; set; } = "";

        [Required, StringLength(1000)]
        public string Mensaje { get; set; } = "";
    }
}
