using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
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
            var baseUrl = config["Valores:UrlAPI"] ?? string.Empty;

            baseUrl = baseUrl.Trim();
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            if (!baseUrl.EndsWith("api/", StringComparison.OrdinalIgnoreCase))
                baseUrl += "api/";

            _urlApi = baseUrl;

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient CrearClienteConToken()
        {
            var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> NotificacionesUsuario()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            using var client = CrearClienteConToken();
            var resp = await client.GetAsync($"Notificaciones/MisNotificaciones/{usuarioId}");

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.Error = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "No autorizado (401). Falta token."
                    : $"No se pudieron cargar las notificaciones. ({(int)resp.StatusCode})";

                return View(new List<NotificacionViewModel>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<NotificacionViewModel>>(json, _jsonOptions) ?? new();

            return View(lista);
        }

        [HttpPost]
        public async Task<JsonResult> MarcarLeida(int notificacionId)
        {
            using var client = CrearClienteConToken();

            var payload = JsonSerializer.Serialize(new { notificacionId });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("Notificaciones/MarcarLeida", content);
            return Json(new { exito = resp.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<JsonResult> MarcarTodas()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            using var client = CrearClienteConToken();

            var payload = JsonSerializer.Serialize(new { usuarioId });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            var resp = await client.PostAsync("Notificaciones/MarcarTodasLeidas", content);
            return Json(new { exito = resp.IsSuccessStatusCode });
        }

        [HttpGet]
        public async Task<IActionResult> NotificacionesAdmin()
        {
            using var client = CrearClienteConToken();
            var resp = await client.GetAsync("Notificaciones/AdminResumen");

            if (!resp.IsSuccessStatusCode)
            {
                ViewBag.Error = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "No autorizado (401). Falta token."
                    : $"No se pudo cargar el resumen. ({(int)resp.StatusCode})";

                return View(new List<NotificacionAdminResumenViewModel>());
            }

            var json = await resp.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<NotificacionAdminResumenViewModel>>(json, _jsonOptions) ?? new();

            return View(lista);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CrearNotificacionMasivaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearNotificacionMasivaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos";
                return View(model);
            }

            using var client = CrearClienteConToken();

            var payload = JsonSerializer.Serialize(new
            {
                destino = string.IsNullOrWhiteSpace(model.Destino) ? "todos" : model.Destino,
                titulo = model.Titulo,
                contenido = model.Contenido,
                tipo = model.Tipo
            });

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("Notificaciones/CrearMasiva", content);

            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                TempData["Error"] = $"No se pudo enviar la notificación. ({(int)resp.StatusCode}) {body}";
                return View(model);
            }

            TempData["Ok"] = "Notificación enviada";
            return RedirectToAction(nameof(NotificacionesAdmin));
        }
    }
}
