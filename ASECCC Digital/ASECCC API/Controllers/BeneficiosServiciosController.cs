using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ASECCC_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BeneficiosServiciosController : ControllerBase
    {
        private readonly IConfiguration _config;

        public BeneficiosServiciosController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string? categoria = null)
        {
            try
            {
                var cs = _config.GetConnectionString("DefaultConnection");
                await using var cn = new SqlConnection(cs);

                await using var cmd = new SqlCommand("dbo.ObtenerBeneficiosServicios", cn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Categoria", (object?)categoria ?? DBNull.Value);

                await cn.OpenAsync();

                var list = new List<BeneficioServicioDto>();
                await using var rd = await cmd.ExecuteReaderAsync();
                while (await rd.ReadAsync())
                {
                    list.Add(new BeneficioServicioDto
                    {
                        BeneficioId = rd.GetInt32(0),
                        Nombre = rd.GetString(1),
                        Descripcion = rd.IsDBNull(2) ? null : rd.GetString(2),
                        Categoria = rd.IsDBNull(3) ? null : rd.GetString(3),
                        Requisitos = rd.IsDBNull(4) ? null : rd.GetString(4),
                        Estado = rd.GetString(5),
                        FechaRegistro = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6)
                    });
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error consultando beneficios/servicios", detail = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var cs = _config.GetConnectionString("DefaultConnection");
                await using var cn = new SqlConnection(cs);

                await using var cmd = new SqlCommand("dbo.ObtenerBeneficioServicioPorId", cn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@BeneficioId", id);

                await cn.OpenAsync();

                await using var rd = await cmd.ExecuteReaderAsync();
                if (!await rd.ReadAsync())
                    return NotFound(new { message = "Beneficio/Servicio no encontrado" });

                var item = new BeneficioServicioDto
                {
                    BeneficioId = rd.GetInt32(0),
                    Nombre = rd.GetString(1),
                    Descripcion = rd.IsDBNull(2) ? null : rd.GetString(2),
                    Categoria = rd.IsDBNull(3) ? null : rd.GetString(3),
                    Requisitos = rd.IsDBNull(4) ? null : rd.GetString(4),
                    Estado = rd.GetString(5),
                    FechaRegistro = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6)
                };

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error consultando el detalle", detail = ex.Message });
            }
        }

        private sealed class BeneficioServicioDto
        {
            public int BeneficioId { get; set; }
            public string Nombre { get; set; } = "";
            public string? Descripcion { get; set; }
            public string? Categoria { get; set; }
            public string? Requisitos { get; set; }
            public string Estado { get; set; } = "";
            public DateTime? FechaRegistro { get; set; }
        }
    }
}
