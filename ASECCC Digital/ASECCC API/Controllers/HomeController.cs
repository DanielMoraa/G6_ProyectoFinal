using ASECCC_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASECCC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        [HttpPost]
        [Route("IniciarSesion")]
        public IActionResult IniciarSesion(UsuarioModel usuario)
        {
            return Ok("HOLA");
        }

    }
}
