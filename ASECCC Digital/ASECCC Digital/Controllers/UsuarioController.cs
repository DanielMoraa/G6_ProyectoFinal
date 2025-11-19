using Microsoft.AspNetCore.Mvc;

namespace ASECCC_Digital.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

<<<<<<< Updated upstream
        public IActionResult Login()
=======
        [HttpGet]
        public IActionResult BuscarDesactivarAsociado()
>>>>>>> Stashed changes
        {
            return View();
        }

<<<<<<< Updated upstream
        public IActionResult RecuperarAcceso()
        {
            return View();
        }
=======

        [HttpPost]
        public IActionResult BuscarAsociado([FromBody] UsuarioModel usuario)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/BuscarAsociado";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new
                {
                    BuscarNombre = usuario.NombreCompleto
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false });

                var datos = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                if (datos == null || datos.UsuarioId == 0)
                    return Json(new { success = false });

                return Json(new
                {
                    success = true,
                    id = datos.UsuarioId,
                    nombre = datos.NombreCompleto,
                    identificacion = datos.Identificacion,
                    correo = datos.CorreoElectronico,
                    telefono = datos.Telefono
                });
            }
        }

        [HttpPost]
        public IActionResult DesactivarAsociado([FromBody] UsuarioModel usuario)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/DesactivarAsociado";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new
                {
                    UsuarioId = usuario.UsuarioId
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Error al comunicarse con el servidor" });

                var resultado = respuesta.Content.ReadFromJsonAsync<int>().Result;

                if (resultado > 0)
                {
                    return Json(new { success = true, message = "Asociado desactivado correctamente" });
                }

                return Json(new { success = false, message = "No se pudo desactivar el asociado" });
            }
        }

        


>>>>>>> Stashed changes
    }
}
