using Dapper;
using Microsoft.Data.SqlClient;

namespace ASECCC_API.Models
{
    public class AhorrosModel
    {
        private readonly string _connectionString;

        public AhorrosModel(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("BDConnection")
                               ?? throw new InvalidOperationException("Cadena de conexión 'BDConnection' no configurada.");
        }

        public async Task<List<AhorroDto>> ObtenerAhorrosPorUsuarioAsync(int usuarioId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  a.ahorroId       AS AhorroId,
                        a.usuarioId      AS UsuarioId,
                        a.tipoAhorroId   AS TipoAhorroId,
                        c.tipoAhorro     AS TipoAhorro,
                        a.montoInicial   AS MontoInicial,
                        a.montoActual    AS MontoActual,
                        a.fechaInicio    AS FechaInicio,
                        a.plazo          AS Plazo,
                        a.estado         AS Estado
                FROM    Ahorros a
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE a.usuarioId = @usuarioId";

            var result = await db.QueryAsync<AhorroDto>(sql, new { usuarioId });
            return result.ToList();
        }

        public async Task<List<AhorroDto>> ObtenerAhorrosAdminAsync(
            string? nombreAsociado,
            int? tipoAhorroId,
            string? estado)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  a.ahorroId       AS AhorroId,
                        a.usuarioId      AS UsuarioId,
                        u.nombreCompleto AS NombreAsociado,
                        a.tipoAhorroId   AS TipoAhorroId,
                        c.tipoAhorro     AS TipoAhorro,
                        a.montoInicial   AS MontoInicial,
                        a.montoActual    AS MontoActual,
                        a.fechaInicio    AS FechaInicio,
                        a.plazo          AS Plazo,
                        a.estado         AS Estado
                FROM    Ahorros a
                INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE   (@nombre IS NULL OR u.nombreCompleto LIKE '%' + @nombre + '%')
                AND     (@tipoAhorroId IS NULL OR a.tipoAhorroId = @tipoAhorroId)
                AND     (@estado IS NULL OR a.estado = @estado)
            ";

            var result = await db.QueryAsync<AhorroDto>(sql, new
            {
                nombre = nombreAsociado,
                tipoAhorroId,
                estado
            });

            return result.ToList();
        }


        public async Task<int> CrearAhorroAsync(CrearAhorroRequest dto)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Ahorros
                    (usuarioId, tipoAhorroId, montoInicial, montoActual, fechaInicio, plazo, estado)
                VALUES
                    (@UsuarioId, @TipoAhorroId, @MontoInicial, @MontoInicial, GETDATE(), @Plazo, 'Activo');

                SELECT CAST(SCOPE_IDENTITY() AS INT);
            ";

            var nuevoId = await db.ExecuteScalarAsync<int>(sql, new
            {
                dto.UsuarioId,
                dto.TipoAhorroId,
                dto.MontoInicial,
                dto.Plazo
            });

            return nuevoId;
        }


        public async Task<AhorroDto?> ObtenerDetalleAsync(int ahorroId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  a.ahorroId       AS AhorroId,
                        a.usuarioId      AS UsuarioId,
                        u.nombreCompleto AS NombreAsociado,
                        a.tipoAhorroId   AS TipoAhorroId,
                        c.tipoAhorro     AS TipoAhorro,
                        a.montoInicial   AS MontoInicial,
                        a.montoActual    AS MontoActual,
                        a.fechaInicio    AS FechaInicio,
                        a.plazo          AS Plazo,
                        a.estado         AS Estado
                FROM    Ahorros a
                INNER JOIN Usuario u ON a.usuarioId = u.usuarioId
                INNER JOIN CatalogoTipoAhorro c ON a.tipoAhorroId = c.tipoAhorroId
                WHERE   a.ahorroId = @ahorroId;
            ";

            return await db.QueryFirstOrDefaultAsync<AhorroDto>(sql, new { ahorroId });
        }


        public async Task<List<AhorroTransaccionDto>> ObtenerHistorialAsync(int ahorroId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  t.transaccionId AS TransaccionId,
                        t.ahorroId      AS AhorroId,
                        t.fecha         AS Fecha,
                        t.tipo          AS Tipo,
                        t.monto         AS Monto,
                        t.descripcion   AS Descripcion
                FROM AhorroTransacciones t
                WHERE t.ahorroId = @ahorroId
                ORDER BY t.fecha DESC;
            ";

            var lista = await db.QueryAsync<AhorroTransaccionDto>(sql, new { ahorroId });
            return lista.ToList();
        }


        public async Task<bool> ModificarMontoAsync(int ahorroId, decimal nuevoMonto)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"UPDATE Ahorros SET montoInicial = @nuevoMonto WHERE ahorroId = @ahorroId";

            var filas = await db.ExecuteAsync(sql, new { ahorroId, nuevoMonto });
            return filas > 0;
        }

        public async Task<bool> EliminarAhorroAsync(int ahorroId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"DELETE FROM Ahorros WHERE ahorroId = @ahorroId";

            var filas = await db.ExecuteAsync(sql, new { ahorroId });
            return filas > 0;
        }
    }
}
