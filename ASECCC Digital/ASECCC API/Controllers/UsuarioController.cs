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
        public IActionResult ObtenerRubrosLiquidacion([FromBody] ObtenerRubrosLiquidacionRequestModel request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var usuario = context.QueryFirstOrDefault<ObtenerRubrosLiquidacionRequestModel>(
                "ObtenerUsuarioParaLiquidacion",
                new { UsuarioId = request.UsuarioId },
                commandType: CommandType.StoredProcedure
            );

            if (usuario == null)
                return Ok(new { success = false, message = "Usuario no encontrado" });

            
            var rubros = context.Query<RubroLiquidacionResponseModel>(
                "ObtenerRubrosActivosPorUsuario",
                new { UsuarioId = request.UsuarioId },
                commandType: CommandType.StoredProcedure
            ).ToList();

            usuario.Rubros = rubros;

            return Ok(new
            {
                success = true,
                usuario.UsuarioId,
                usuario.NombreCompleto,
                rubros
            });
        }




        [HttpPost]
        [Route("LiquidarRubro")]
        public IActionResult LiquidarRubro([FromBody] LiquidarRubroRequestModel request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", request.UsuarioId);
            parametros.Add("@TipoRubro", request.TipoRubro);
            parametros.Add("@IdRubro", request.IdRubro);

            var resultado = context.QueryFirstOrDefault<int>(
                "LiquidarRubroAsociado",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            return Ok(new
            {
                success = resultado > 0,
                filasAfectadas = resultado
            });
        }


        [HttpPost]
        [Route("ListarAsociadosAdmin")]
        public IActionResult ListarAsociadosAdmin([FromBody] DataTableRequest request)
        {
            using var context =
                new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@Search", request.search?.value);
            parametros.Add("@Start", request.start);
            parametros.Add("@Length", request.length);

            using var multi = context.QueryMultiple(
                "ListarAsociadosAdmin",
                parametros,
                commandType: CommandType.StoredProcedure
            );

            var data = multi.Read().ToList();

            var total = multi.ReadFirst<int>();

            return Ok(new
            {
                draw = request.draw,
                recordsTotal = total,
                recordsFiltered = total,
                data
            });
        }



    }
}
