using System;

namespace ASECCC_Digital.Models
{
    public class NotificacionViewModel
    {
        public int NotificacionId { get; set; }
        public int UsuarioId { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
        public bool Leido { get; set; }
    }
}
