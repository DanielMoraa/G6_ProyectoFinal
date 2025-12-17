using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using Utiles;

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

        public IActionResult Principal()
        {

            return View();
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
                var helper = new Helper();
                usuario.Contrasena = helper.Encrypt(usuario.Contrasena);
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
        public IActionResult BuscarAsociado([FromBody] UsuarioModel usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                return Json(new { success = false });

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/BuscarAsociado";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        HttpContext.Session.GetString("Token")
                    );

                var payload = new
                {
                    BuscarNombre = usuario.NombreCompleto
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false });

                var lista = respuesta.Content
                    .ReadFromJsonAsync<List<AsociadoBusquedaViewModel>>()
                    .Result;

                if (lista == null || lista.Count == 0)
                    return Json(new { success = false });

                return Json(new { success = true, data = lista });
            }
        }


        [HttpPost]
        public IActionResult DesactivarAsociado([FromBody] UsuarioModel usuario)
        {
            if (usuario.UsuarioId <= 0)
                return Json(new { success = false, message = "Usuario inválido" });

            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/DesactivarAsociado";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        HttpContext.Session.GetString("Token")
                    );

                var payload = new
                {
                    UsuarioId = usuario.UsuarioId
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Error al comunicarse con el servidor" });

                var resultado = respuesta.Content.ReadFromJsonAsync<int>().Result;

                if (resultado > 0)
                    return Json(new { success = true, message = "Asociado desactivado correctamente" });

                return Json(new { success = false, message = "No se pudo desactivar el asociado" });
            }
        }


        [HttpGet]
        public IActionResult LiquidarAsociado()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ObtenerRubrosLiquidacion([FromBody] UsuarioModel usuario)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/ObtenerRubrosLiquidacion";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new
                {
                    BuscarNombre = usuario.NombreCompleto
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Error al comunicarse con el servidor" });

                var datos = respuesta.Content.ReadFromJsonAsync<RubrosLiquidacionModel>().Result;

                if (datos == null || datos.UsuarioId == 0)
                    return Json(new { success = false, message = "Asociado no encontrado" });

                return Json(new
                {
                    success = true,
                    usuarioId = datos.UsuarioId,
                    nombreCompleto = datos.NombreCompleto,
                    rubros = datos.Rubros
                });
            }
        }

        [HttpPost]
        public IActionResult LiquidarRubro([FromBody] LiquidarRubroModel liquidacion)
        {
            using (var client = _http.CreateClient())
            {
                var url = _configuration["Valores:UrlAPI"] + "Usuario/LiquidarRubro";
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", HttpContext.Session.GetString("Token"));

                var payload = new
                {
                    liquidacion.UsuarioId,
                    liquidacion.TipoRubro,
                    liquidacion.IdRubro
                };

                var respuesta = client.PostAsJsonAsync(url, payload).Result;

                if (!respuesta.IsSuccessStatusCode)
                    return Json(new { success = false, message = "Error al comunicarse con el servidor" });

                // Leer como string primero para debug
                var jsonString = respuesta.Content.ReadAsStringAsync().Result;

                // Deserializar usando System.Text.Json
                using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
                var filasAfectadas = doc.RootElement.GetProperty("filasAfectadas").GetInt32();

                if (filasAfectadas > 0)
                {
                    return Json(new { success = true, message = "Rubro liquidado correctamente" });
                }

                return Json(new { success = false, message = "No se pudo liquidar el rubro" });
            }
        }
    }
}
