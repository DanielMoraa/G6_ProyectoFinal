using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ASECCC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficiosServiciosController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BeneficiosServiciosController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult ObtenerBeneficiosServicios(string? categoria = null)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Categoria", string.IsNullOrEmpty(categoria) ? null : categoria, DbType.String);

                var resultado = context.Query<BeneficioServicioResponseModel>(
                    "ObtenerBeneficiosServicios",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(resultado);
            }
        }

        [HttpGet("{id}")]
        public IActionResult ObtenerBeneficioServicioPorId(int id)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@BeneficioId", id);

                var resultado = context.QueryFirstOrDefault<BeneficioServicioResponseModel>(
                    "ObtenerBeneficioServicioPorId",
                    parametros,
                    commandType: CommandType.StoredProcedure
                );

                if (resultado != null)
                {
                    return Ok(resultado);
                }

                return NotFound();
            }
        }
    }
}