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
    public class AportesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AportesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("ObtenerAportesPorUsuario")]
        public IActionResult ObtenerAportesPorUsuario(int usuarioId)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", usuarioId);

                var resultado = context.Query<AporteResponseModel>(
                    "ObtenerAportesPorUsuario",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(resultado);
            }
        }

        [HttpGet]
        [Route("ObtenerAportesAdmin")]
        public IActionResult ObtenerAportesAdmin(
            string? nombreAsociado,
            string? tipoAporte,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                // Opcional: normalizar vacíos a null
                var nombre = string.IsNullOrWhiteSpace(nombreAsociado) ? null : nombreAsociado;
                var tipo = string.IsNullOrWhiteSpace(tipoAporte) ? null : tipoAporte;

                var parametros = new DynamicParameters();
                parametros.Add("@Nombre", nombre);
                parametros.Add("@TipoAporte", tipo);
                parametros.Add("@FechaDesde", fechaDesde);
                parametros.Add("@FechaHasta", fechaHasta);

                var resultado = context.Query<AporteAdminResponseModel>(
                    "ObtenerAportesAdmin",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(resultado);
            }
        }

        [HttpPost]
        [Route("CrearAporte")]
        public IActionResult CrearAporte([FromBody] CrearAporteRequestModel request)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@UsuarioId", request.UsuarioId);
                parametros.Add("@TipoAporte", request.TipoAporte);
                parametros.Add("@Monto", request.Monto);

                var nuevoId = context.QuerySingle<int>(
                    "CrearAporte",
                    parametros,
                    commandType: CommandType.StoredProcedure);

                return Ok(nuevoId);
            }
        }
    }
}
