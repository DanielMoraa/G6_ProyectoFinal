using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ASECCC_API.Controllers
{
    [Authorize]
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

        [HttpPost]
        [Route("BuscarAsociado")]
        public IActionResult BuscarAsociado(BuscarAsociadoRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@BuscarNombre", usuario.BuscarNombre);

                var resultado = context.Query(
                    "BuscarAsociadoPorNombre",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado); 
            }
        }


        [HttpPost]
        [Route("DesactivarAsociado")]
        public IActionResult DesactivarAsociado(DesactivarAsociadoRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("UsuarioId", usuario.UsuarioId);

                var resultado = context.QueryFirstOrDefault<int>(
                    "DesactivarAsociado",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("ObtenerRubrosLiquidacion")]
        public IActionResult ObtenerRubrosLiquidacion(BuscarAsociadoLiquidacionRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("BuscarNombre", usuario.BuscarNombre);

                var resultado = context.QueryFirstOrDefault<ObtenerRubrosLiquidacionResponseModel>(
                    "ObtenerRubrosParaLiquidacion",
                    parametros,
                    commandType: System.Data.CommandType.StoredProcedure
                );

                if (resultado == null)
                {
                    return Ok(new ObtenerRubrosLiquidacionResponseModel());
                }
                var rubros = context.Query<RubroLiquidacionResponseModel>(
                    "ObtenerDetalleRubrosLiquidacion",
                    new { UsuarioId = resultado.UsuarioId },
                    commandType: System.Data.CommandType.StoredProcedure
                ).ToList();

                resultado.Rubros = rubros;

                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("LiquidarRubro")]
        public IActionResult LiquidarRubro(LiquidarRubroRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("UsuarioId", usuario.UsuarioId);
                parametros.Add("TipoRubro", usuario.TipoRubro);
                parametros.Add("IdRubro", usuario.IdRubro);

                var resultado = context.QueryFirstOrDefault<int>(
                    "LiquidarRubroAsociado",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

               
                return Ok(new { filasAfectadas = resultado });
            }
        }
    }
}
