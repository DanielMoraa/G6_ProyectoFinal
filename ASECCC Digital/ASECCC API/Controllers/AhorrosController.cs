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
    public class AhorrosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AhorrosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerAhorrosPorUsuario")]
        public IActionResult ObtenerAhorrosPorUsuario(int usuarioId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", usuarioId);

            var resultado = context.Query<AhorroResponseModel>(
                "ObtenerAhorrosPorUsuario",
                parametros,
                commandType: CommandType.StoredProcedure);

            return Ok(resultado);
        }

        [HttpGet]
        [Route("ObtenerAhorrosAdmin")]
        public IActionResult ObtenerAhorrosAdmin(string? nombreAsociado, int? tipoAhorroId, string? estado)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var nombre = string.IsNullOrWhiteSpace(nombreAsociado) ? null : nombreAsociado;
            var est = string.IsNullOrWhiteSpace(estado) ? null : estado;

            var parametros = new DynamicParameters();
            parametros.Add("@Nombre", nombre);
            parametros.Add("@TipoAhorroId", tipoAhorroId);
            parametros.Add("@Estado", est);

            var resultado = context.Query<AhorroResponseModel>(
                "ObtenerAhorrosAdmin",
                parametros,
                commandType: CommandType.StoredProcedure);

            return Ok(resultado);
        }

        [HttpGet]
        [Route("ObtenerDetalleAhorro")]
        public IActionResult ObtenerDetalleAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var resultado = context.QueryFirstOrDefault<AhorroResponseModel>(
                "ObtenerDetalleAhorro",
                parametros,
                commandType: CommandType.StoredProcedure);

            if (resultado == null) return NotFound("No existe el ahorro.");
            return Ok(resultado);
        }

        [HttpGet]
        [Route("ObtenerTransaccionesAhorro")]
        public IActionResult ObtenerTransaccionesAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var resultado = context.Query<AhorroTransaccionResponseModel>(
                "ObtenerTransaccionesAhorro",
                parametros,
                commandType: CommandType.StoredProcedure);

            return Ok(resultado);
        }

        [HttpPost]
        [Route("CrearAhorro")]
        public IActionResult CrearAhorro([FromBody] CrearAhorroRequest request)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@UsuarioId", request.UsuarioId);
            parametros.Add("@TipoAhorroId", request.TipoAhorroId);
            parametros.Add("@MontoInicial", request.MontoInicial);
            parametros.Add("@Plazo", request.Plazo);

            var nuevoId = context.QuerySingle<int>(
                "CrearAhorro",
                parametros,
                commandType: CommandType.StoredProcedure);

            return Ok(nuevoId);
        }

        [HttpPut]
        [Route("ModificarMontoAhorro")]
        public IActionResult ModificarMontoAhorro(int ahorroId, decimal nuevoMonto)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);
            parametros.Add("@NuevoMonto", nuevoMonto);

            var filas = context.Execute(
                "ModificarMontoAhorro",
                parametros,
                commandType: CommandType.StoredProcedure);

            if (filas == 0) return NotFound("No existe el ahorro.");

            return Ok(filas);
        }

        [HttpGet]
        [Route("ObtenerCatalogoTipoAhorro")]
        public IActionResult ObtenerCatalogoTipoAhorro()
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var resultado = context.Query<CatalogoTipoAhorroResponseModel>(
                "ObtenerCatalogoTipoAhorro",
                commandType: CommandType.StoredProcedure);

            return Ok(resultado);
        }

        [HttpDelete]
        [Route("EliminarAhorro")]
        public IActionResult EliminarAhorro(int ahorroId)
        {
            using var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]);

            var parametros = new DynamicParameters();
            parametros.Add("@AhorroId", ahorroId);

            var filas = context.Execute(
                "EliminarAhorro",
                parametros,
                commandType: CommandType.StoredProcedure);

            if (filas == 0) return NotFound("No existe el ahorro.");

            return Ok(filas);
        }
    }
}
