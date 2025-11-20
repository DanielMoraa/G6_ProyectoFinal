using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASECCC_Digital.Controllers
{
    public class AportesController : Controller
    {
        private readonly string _urlApi;
        private readonly JsonSerializerOptions _jsonOptions;

        public AportesController(IConfiguration config)
        {
            _urlApi = config["Valores:UrlAPI"] ?? string.Empty; // Ej: "https://localhost:7119/api/"
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [HttpGet]
        public async Task<IActionResult> MisAportes()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 1;

            var lista = new List<AporteViewModel>();

            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };
            var response = await client.GetAsync($"Aportes/MisAportes/{usuarioId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                lista = JsonSerializer.Deserialize<List<AporteViewModel>>(json, _jsonOptions)
                        ?? new List<AporteViewModel>();
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar los aportes";
            }

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Gestionar(
            string? nombreAsociado,
            string? tipoAporte,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var lista = new List<AporteViewModel>();

            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

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

                lista = JsonSerializer.Deserialize<List<AporteViewModel>>(json, _jsonOptions)
                        ?? new List<AporteViewModel>();
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar los aportes";
            }

            return View(lista);
        }
    }
}
