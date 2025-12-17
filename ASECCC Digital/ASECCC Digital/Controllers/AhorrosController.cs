using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ASECCC_Digital.Controllers
{
    public class AhorrosController : Controller
    {
        private readonly string _urlApi;
        private readonly JsonSerializerOptions _jsonOptions;

        public AhorrosController(IConfiguration config)
        {
            _urlApi = config["Valores:UrlAPI"] ?? string.Empty;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private HttpClient CrearClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var token = HttpContext.Session.GetString("Token");
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> MisAhorros()
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            var lista = new List<AhorroViewModel>();

            using var client = CrearClient();

            var response = await client.GetAsync($"Ahorros/ObtenerAhorrosPorUsuario?usuarioId={usuarioId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<AhorroViewModel>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = "No se cargaron los ahorros";
            }

            var respTipos = await client.GetAsync("Ahorros/ObtenerCatalogoTipoAhorro");
            if (respTipos.IsSuccessStatusCode)
            {
                var jsonTipos = await respTipos.Content.ReadAsStringAsync();
                var tipos = JsonSerializer.Deserialize<List<CatalogoTipoAhorroViewModel>>(jsonTipos, _jsonOptions) ?? new();

                ViewBag.TiposAhorro = tipos.Select(x => new SelectListItem
                {
                    Value = x.TipoAhorroId.ToString(),
                    Text = x.TipoAhorro
                }).ToList();
            }
            else
            {
                ViewBag.TiposAhorro = new List<SelectListItem>();
            }

            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> CrearAhorro(int TipoAhorroId, decimal MontoInicial, int? Plazo)
        {
            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            if (usuarioId == 0)
            {
                TempData["Error"] = "Sesión inválida. Vuelve a iniciar sesión.";
                return RedirectToAction("MisAhorros");
            }

            using var client = CrearClient();

            var payload = new
            {
                UsuarioId = usuarioId,
                TipoAhorroId = TipoAhorroId,
                MontoInicial = MontoInicial,
                Plazo = Plazo
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var resp = await client.PostAsync("Ahorros/CrearAhorro", content);

            if (!resp.IsSuccessStatusCode)
                TempData["Error"] = "No se pudo crear el ahorro.";

            return RedirectToAction("MisAhorros");
        }

        [HttpGet]
        public async Task<IActionResult> Gestionar(string? nombreAsociado, int? tipoAhorroId, string? estado)
        {
            var lista = new List<AhorroViewModel>();

            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(nombreAsociado)) qs.Add($"nombreAsociado={Uri.EscapeDataString(nombreAsociado)}");
            if (tipoAhorroId.HasValue) qs.Add($"tipoAhorroId={tipoAhorroId.Value}");
            if (!string.IsNullOrWhiteSpace(estado)) qs.Add($"estado={Uri.EscapeDataString(estado)}");

            var url = "Ahorros/ObtenerAhorrosAdmin" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");

            using var client = CrearClient();
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<AhorroViewModel>>(json, _jsonOptions) ?? new();
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar los ahorros (admin).";
            }

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int ahorroId)
        {
            using var client = CrearClient();
            var response = await client.GetAsync($"Ahorros/ObtenerDetalleAhorro?ahorroId={ahorroId}");

            if (!response.IsSuccessStatusCode)
                return PartialView("_Error", "Error al cargar el detalle.");

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<AhorroViewModel>(json, _jsonOptions);

            return PartialView("DetalleAhorro", model);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int ahorroId)
        {
            using var client = CrearClient();
            var response = await client.GetAsync($"Ahorros/ObtenerTransaccionesAhorro?ahorroId={ahorroId}");

            if (!response.IsSuccessStatusCode)
                return PartialView("_Error", "Error al cargar el historial.");

            var json = await response.Content.ReadAsStringAsync();
            var model = JsonSerializer.Deserialize<List<AhorroTransaccionViewModel>>(json, _jsonOptions) ?? new();

            return PartialView("HistorialAhorro", model);
        }

        [HttpPost]
        public async Task<JsonResult> ModificarMonto(int ahorroId, string nuevoMonto)
        {
            using var client = CrearClient();

            nuevoMonto = (nuevoMonto ?? "").Replace(",", ".");
            if (!decimal.TryParse(nuevoMonto, NumberStyles.Any, CultureInfo.InvariantCulture, out var montoDecimal))
                return Json(new { exito = false });

            var montoUrl = montoDecimal.ToString(CultureInfo.InvariantCulture);

            var resp = await client.PutAsync($"Ahorros/ModificarMontoAhorro?ahorroId={ahorroId}&nuevoMonto={montoUrl}", null);
            return Json(new { exito = resp.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<JsonResult> Eliminar(int ahorroId)
        {
            using var client = CrearClient();
            var resp = await client.DeleteAsync($"Ahorros/EliminarAhorro?ahorroId={ahorroId}");
            return Json(new { exito = resp.IsSuccessStatusCode });
        }
    }

    public class CatalogoTipoAhorroViewModel
    {
        public int TipoAhorroId { get; set; }
        public string TipoAhorro { get; set; } = "";
    }
}
