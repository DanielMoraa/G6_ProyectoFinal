using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ASECCC_Digital.Controllers
{
    public class NotificacionesController : Controller
    {
        private readonly string _urlApi;
        private readonly JsonSerializerOptions _jsonOptions;

        public NotificacionesController(IConfiguration config)
        {
            _urlApi = config["Valores:UrlAPI"] ?? string.Empty; // "https://localhost:7119/api/"
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<IActionResult> NotificacionesUsuario()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            var lista = new List<NotificacionViewModel>();

            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var response = await client.GetAsync($"Notificaciones/Usuario/{usuarioId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<NotificacionViewModel>>(json, _jsonOptions)
                        ?? new List<NotificacionViewModel>();
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar las notificaciones.";
            }

            return View(lista);
        }

        [HttpPost]
        public async Task<JsonResult> MarcarLeida(int notificacionId)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var response = await client.PostAsync($"Notificaciones/MarcarLeida/{notificacionId}", null);

            return Json(new { exito = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<JsonResult> MarcarTodas()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;


            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var response = await client.PostAsync($"Notificaciones/MarcarTodas/{usuarioId}", null);

            return Json(new { exito = response.IsSuccessStatusCode });
        }
    }
}
