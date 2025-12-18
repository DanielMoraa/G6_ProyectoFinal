using ASECCC_Digital.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace ASECCC_Digital.Controllers
{
    public class BeneficiosyServiciosController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public BeneficiosyServiciosController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? categoria = null)
        {
            var urlApi = _configuration["Valores:UrlAPI"] + "BeneficiosServicios";

            if (!string.IsNullOrWhiteSpace(categoria))
            {
                urlApi += $"?categoria={Uri.EscapeDataString(categoria)}";
            }

            try
            {
                var response = await _httpClient.GetAsync(urlApi);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Mensaje = "Error al consultar el API";
                    return View(new List<BeneficioServicioModel>());
                }

                var datos = await response.Content
                    .ReadFromJsonAsync<List<BeneficioServicioModel>>();

                return View(datos ?? new List<BeneficioServicioModel>());
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message;
                return View(new List<BeneficioServicioModel>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var urlApi = _configuration["Valores:UrlAPI"] + $"BeneficiosServicios/{id}";

            try
            {
                var response = await _httpClient.GetAsync(urlApi);

                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                var dato = await response.Content
                    .ReadFromJsonAsync<BeneficioServicioModel>();

                if (dato == null)
                {
                    return RedirectToAction("Index");
                }

                return View(dato);
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
    }
}
