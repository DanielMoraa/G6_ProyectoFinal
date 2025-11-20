using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASECCC_Digital.Controllers
{
    public class AhorrosController : Controller
    {
        private readonly string _urlApi;

        public AhorrosController(IConfiguration config)
        {
            _urlApi = config["Valores:UrlAPI"]!; // Ej: "https://localhost:7119/api/"
        }


        [HttpGet]
        public async Task<IActionResult> MisAhorros()
        {
            int usuarioId = 1;
            var lista = new List<AhorroViewModel>();

            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };
            var response = await client.GetAsync($"Ahorros/MisAhorros/{usuarioId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                lista = JsonSerializer.Deserialize<List<AhorroViewModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else
            {
                ViewBag.Error = "No se pudieron cargar los ahorros";
            }

            return View(lista);
        }


        [HttpGet]
        public IActionResult Gestionar()
        {
            return View(new List<AhorroViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Gestionar(string? nombreAsociado, int? tipoAhorroId, string? estado)
        {
            var lista = new List<AhorroViewModel>();

            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            string query =
                $"Ahorros/Gestionar?nombreAsociado={nombreAsociado}&tipoAhorroId={tipoAhorroId}&estado={estado}";

            var response = await client.GetAsync(query);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                lista = JsonSerializer.Deserialize<List<AhorroViewModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            else
            {
                ViewBag.Error = "No se lograron cargar los datos.";
            }

            return View(lista);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int ahorroId)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var response = await client.GetAsync($"Ahorros/Detalle/{ahorroId}");

            if (!response.IsSuccessStatusCode)
                return PartialView("_Error", "Error.");

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<AhorroViewModel>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return PartialView("_DetalleAhorro", model);
        }

        [HttpGet]
        public async Task<IActionResult> Historial(int ahorroId)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var response = await client.GetAsync($"Ahorros/Historial/{ahorroId}");

            if (!response.IsSuccessStatusCode)
                return PartialView("_Error", "Error al cargar historial.");

            var json = await response.Content.ReadAsStringAsync();

            var model = JsonSerializer.Deserialize<List<AhorroTransaccionViewModel>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return PartialView("_HistorialAhorro", model);
        }

        [HttpPost]
        public async Task<JsonResult> ModificarMonto(int ahorroId, decimal nuevoMonto)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var resp = await client.PutAsync(
                $"Ahorros/ModificarMonto?ahorroId={ahorroId}&nuevoMonto={nuevoMonto}",
                null);

            return Json(new { exito = resp.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<JsonResult> Eliminar(int ahorroId)
        {
            using var client = new HttpClient { BaseAddress = new Uri(_urlApi) };

            var resp = await client.DeleteAsync($"Ahorros/Eliminar/{ahorroId}");

            return Json(new { exito = resp.IsSuccessStatusCode });
        }
    }
}
