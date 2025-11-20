using Dapper;
using Microsoft.Data.SqlClient;

namespace ASECCC_API.Models
{
    public class AportesModel
    {
        private readonly string _connectionString;

        public AportesModel(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("BDConnection")
                               ?? throw new InvalidOperationException("Cadena de conexión 'BDConnection' no configurada.");
        }

        // Aportes de  asociado
        public async Task<List<AporteDto>> ObtenerAportesPorUsuarioAsync(int usuarioId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  a.aporteId      AS AporteId,
                        a.usuarioId     AS UsuarioId,
                        a.tipoAporte    AS TipoAporte,
                        a.monto         AS Monto,
                        a.fechaRegistro AS FechaRegistro
                FROM    Aportes a
                WHERE   a.usuarioId = @usuarioId
                ORDER BY a.fechaRegistro DESC";

            var result = await db.QueryAsync<AporteDto>(sql, new { usuarioId });
            return result.ToList();
        }

        // Aportes admin 
        public async Task<List<AporteDto>> ObtenerAportesAdminAsync(
            string? nombreAsociado,
            string? tipoAporte,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  a.aporteId          AS AporteId,
                        a.usuarioId         AS UsuarioId,
                        u.nombreCompleto    AS NombreAsociado,
                        a.tipoAporte        AS TipoAporte,
                        a.monto             AS Monto,
                        a.fechaRegistro     AS FechaRegistro
                FROM    Aportes a
                INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                WHERE   (@nombre IS NULL OR u.nombreCompleto LIKE '%' + @nombre + '%')
                AND     (@tipoAporte IS NULL OR a.tipoAporte = @tipoAporte)
                AND     (@fechaDesde IS NULL OR a.fechaRegistro >= @fechaDesde)
                AND     (@fechaHasta IS NULL OR a.fechaRegistro <= @fechaHasta)
                ORDER BY a.fechaRegistro DESC";

            var result = await db.QueryAsync<AporteDto>(sql, new
            {
                nombre = nombreAsociado,
                tipoAporte,
                fechaDesde,
                fechaHasta
            });

            return result.ToList();
        }

        // Crea aporte
        public async Task<int> CrearAporteAsync(CrearAporteRequest dto)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Aportes
                    (usuarioId, tipoAporte, monto, fechaRegistro)
                VALUES
                    (@UsuarioId, @TipoAporte, @Monto, GETDATE());

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            var nuevoId = await db.ExecuteScalarAsync<int>(sql, new
            {
                dto.UsuarioId,
                dto.TipoAporte,
                dto.Monto
            });

            return nuevoId;
        }
    }
}
