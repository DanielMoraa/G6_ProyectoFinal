using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ASECCC_Digital.Controllers
{
    [Seguridad]
    public class UsuarioController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;
        public UsuarioController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult EditarPerfil()
        {
            using (var context = _http.CreateClient())
            {
                var consecutivo = HttpContext.Session.GetInt32("UsuarioId");
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ConsultarAsociado?UsuarioId=" + consecutivo;
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;
                    return View(datosApi);
                }

                ViewBag.Mensaje = "No hay información registrada";
                return View(new UsuarioModel());
            }
        }


        [HttpPost]
        public IActionResult EditarPerfil(UsuarioModel usuario)
        {
            ViewBag.Mensaje = "La información no se ha actualizado correctamente";
            usuario.UsuarioId = (int)HttpContext.Session.GetInt32("UsuarioId")!;

            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ActualizarPerfil";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                    {
                        ViewBag.Mensaje = "La información se ha actualizado correctamente";
                        HttpContext.Session.SetString("NombreUsuario", usuario.NombreCompleto);
                    }
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult Seguridad()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Seguridad(UsuarioModel usuario)
        {
            ViewBag.Mensaje = "La información no se ha actualizado correctamente";
            usuario.UsuarioId = (int)HttpContext.Session.GetInt32("UsuarioId")!;

            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Usuario/ActualizarSeguridad";
                context.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));
                var respuesta = context.PutAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<int>().Result;

                    if (datosApi > 0)
                        ViewBag.Mensaje = "La información se ha actualizado correctamente";
                }

                return View();
            }
        }

        [HttpGet]
        public IActionResult BuscarDesactivarAsociado()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BuscarAsociado(UsuarioModel usuario)
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
        public IActionResult DesactivarAsociado(UsuarioModel usuario)
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


    }
}
