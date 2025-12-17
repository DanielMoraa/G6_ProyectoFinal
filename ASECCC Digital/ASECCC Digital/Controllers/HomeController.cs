using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Utiles;

namespace ASECCC_Digital.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _configuration;
        public HomeController(IHttpClientFactory http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        #region Actions de Iniciar Sesión

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UsuarioModel usuario)
        {
            var helper = new Helper();
            usuario.Contrasena = helper.Encrypt(usuario.Contrasena);
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Home/ValidarSesion";
                var respuesta = context.PostAsJsonAsync(urlApi, usuario).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                    if (datosApi != null)
                    {
                        HttpContext.Session.SetInt32("UsuarioId", datosApi.UsuarioId);
                        HttpContext.Session.SetString("NombreCompleto", datosApi.NombreCompleto);
                        HttpContext.Session.SetString("NombrePerfil", datosApi.NombrePerfil);
                        HttpContext.Session.SetInt32("PerfilId", datosApi.PerfilId);
                        HttpContext.Session.SetString("Token", datosApi.Token);
                        return RedirectToAction("Principal", "Home");
                    }
                }

                ViewBag.Mensaje = "No se ha validado la información";
                return View();
            }
        }

        #endregion

        #region Actions de Registrar Usuario

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistroAsync([FromBody] UsuarioModel usuario)
        {
            try
            {
                var helper = new Helper();
                usuario.Contrasena = helper.Encrypt(usuario.Contrasena);
                using (var context = _http.CreateClient())
                {
                    var urlApi = _configuration["Valores:UrlAPI"] + "Home/Registro";
                    var respuesta = await context.PostAsJsonAsync(urlApi, usuario);
                    var contenido = await respuesta.Content.ReadAsStringAsync();
                    Console.WriteLine(contenido);
                    if (respuesta.IsSuccessStatusCode)
                    {
                        var datosApi = await respuesta.Content.ReadFromJsonAsync<int>();
                        if (datosApi > 0)
                        {
                            return Json(new { success = true, message = "Registro exitoso" });
                        }
                    }
                    return Json(new { success = false, message = "No se ha registrado la información" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Error al procesar la solicitud" });
            }
        }

        #endregion

        #region Actions de Recuperar Acceso

        [HttpGet]
        public IActionResult RecuperarAcceso()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarAcceso(UsuarioModel usuario)
        {
            using (var context = _http.CreateClient())
            {
                var urlApi = _configuration["Valores:UrlAPI"] + "Home/ValidarUsuario?CorreoElectronico=" + usuario.CorreoElectronico;
                var respuesta = context.GetAsync(urlApi).Result;

                if (respuesta.IsSuccessStatusCode)
                {
                    var datosApi = respuesta.Content.ReadFromJsonAsync<UsuarioModel>().Result;

                    if (datosApi != null)
                        return RedirectToAction("Login", "Home");
                }

                ViewBag.Mensaje = "No se ha recuperado el acceso";
                return View();
            }
        }

        #endregion

        [Seguridad]
        [HttpGet]
        public IActionResult Principal()
        {
            return View();
        }

        [Seguridad]
        [HttpGet]
        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
