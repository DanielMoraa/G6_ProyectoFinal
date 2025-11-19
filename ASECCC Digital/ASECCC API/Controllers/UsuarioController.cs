using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ASECCC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public UsuarioController(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        [HttpGet]
        [Route("ConsultarAsociado")]
        public IActionResult ConsultarAsociado(int UsuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("UsuarioId", UsuarioId);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ConsultarAsociado", parametros);
                return Ok(resultado);
            }
        }


        [HttpPut]
        [Route("ActualizarPerfil")]
        public IActionResult ActualizarPerfil(PerfilRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("UsuarioId", usuario.UsuarioId);
                parametros.Add("NombreCompleto", usuario.NombreCompleto);
                parametros.Add("Identificacion", usuario.Identificacion);
                parametros.Add("FechaNacimiento", usuario.FechaNacimiento);
                parametros.Add("CorreoElectronico", usuario.CorreoElectronico);
                parametros.Add("Telefono", usuario.Telefono);
                parametros.Add("Direccion", usuario.Direccion);

                var resultado = context.Execute("ActualizarPerfil", parametros);
                return Ok(resultado);
            }
        }


        [HttpPut]
        [Route("ActualizarSeguridad")]
        public IActionResult ActualizarSeguridad(SeguridadRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("UsuarioId", usuario.UsuarioId);
                parametros.Add("Contrasena", usuario.Contrasena);

                var resultado = context.Execute("ActualizarContrasena", parametros);
                return Ok(resultado);
            }
        }
    }
}
