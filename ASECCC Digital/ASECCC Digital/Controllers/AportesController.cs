using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ASECCC_Digital.Controllers
{
    public class AportesController : Controller
    {
        private readonly string _urlApi;
        private readonly JsonSerializerOptions _jsonOptions;

        public AportesController(IConfiguration config)
        {
            _urlApi = config["Valores:UrlAPI"] ?? string.Empty; // ej: "https://localhost:7119/api/"
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient CrearClienteConToken()
        {
            var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> MisAportes()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            var lista = new List<AporteViewModel>();

            using var client = CrearClienteConToken();
            var response = await client.GetAsync($"Aportes/MisAportes/{usuarioId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<AporteViewModel>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "No autorizado (401). Falta enviar el token al API."
                    : $"No se pudieron cargar los aportes. ({(int)response.StatusCode})";
            }

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Gestionar(string? nombreAsociado, string? tipoAporte, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var lista = new List<AporteViewModel>();

            using var client = CrearClienteConToken();

            string qsNombre = nombreAsociado ?? string.Empty;
            string qsTipo = tipoAporte ?? string.Empty;
            string qsDesde = fechaDesde.HasValue ? fechaDesde.Value.ToString("yyyy-MM-dd") : string.Empty;
            string qsHasta = fechaHasta.HasValue ? fechaHasta.Value.ToString("yyyy-MM-dd") : string.Empty;

            string query =
                $"Aportes/Gestionar" +
                $"?nombreAsociado={Uri.EscapeDataString(qsNombre)}" +
                $"&tipoAporte={Uri.EscapeDataString(qsTipo)}" +
                $"&fechaDesde={qsDesde}" +
                $"&fechaHasta={qsHasta}";

            var response = await client.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<AporteViewModel>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "No autorizado (401). Falta enviar el token al API."
                    : $"No se pudieron cargar los aportes. ({(int)response.StatusCode})";
            }

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> CrearAporte([FromBody] CrearAporteRequestVM request)
        {
            if (request == null) return BadRequest("Request inválido");
            if (request.UsuarioId <= 0) return BadRequest("UsuarioId inválido");
            if (string.IsNullOrWhiteSpace(request.TipoAporte)) return BadRequest("TipoAporte requerido");
            if (request.Monto <= 0) return BadRequest("Monto inválido");

            using var client = CrearClienteConToken();

            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API:  https://localhost:7119/api/Aportes
            var response = await client.PostAsync("Aportes", content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, body);

            return Content(body, "application/json");
        }

        [HttpGet]
        public async Task<IActionResult> Historial(string tipoAporte)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            using var client = CrearClienteConToken();
            var response = await client.GetAsync($"Aportes/MisAportes/{usuarioId}");

            if (!response.IsSuccessStatusCode)
                return Content("<div class='alert alert-danger mb-0'>No se pudo cargar el historial.</div>", "text/html");

            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<AporteViewModel>>(json, _jsonOptions) ?? new();

            var filtrada = lista
                .Where(a => string.Equals(a.TipoAporte, tipoAporte, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_Historial", filtrada);
        }
    }

    public class CrearAporteRequestVM
    {
        public int UsuarioId { get; set; }
        public string TipoAporte { get; set; } = string.Empty;
        public decimal Monto { get; set; }
    }
}
