using Microsoft.AspNetCore.Mvc;

namespace ASECCC_Digital.Controllers
{
    public class UsuarioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Registro()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult RecuperarAcceso()
        {
            return View();
        }
    }
}
