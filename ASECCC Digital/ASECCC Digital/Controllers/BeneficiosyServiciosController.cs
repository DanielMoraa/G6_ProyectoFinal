using Microsoft.AspNetCore.Mvc;
using ASECCC_Digital.Services;

namespace ASECCC_Digital.Controllers
{
    public class BeneficiosyServiciosController : Controller
    {
        private readonly IBeneficiosServiciosClient _client;

        public BeneficiosyServiciosController(IBeneficiosServiciosClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? categoria = null)
        {
            var data = await _client.GetAllAsync(categoria);
            ViewBag.Categoria = categoria;
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var item = await _client.GetByIdAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }
    }
}
