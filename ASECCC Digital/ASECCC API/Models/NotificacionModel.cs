using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ASECCC_API.Models
{
    public class NotificacionModel
    {
        private readonly string _connectionString;

        public NotificacionModel(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("BDConnection")
                               ?? throw new InvalidOperationException("Cadena de conexión 'BDConnection' no configurada.");
        }

        public async Task<List<NotificacionDto>> ObtenerNotificacionesPorUsuarioAsync(int usuarioId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"
                SELECT  n.notificacionId AS NotificacionId,
                        n.usuarioId      AS UsuarioId,
                        n.titulo         AS Titulo,
                        n.mensaje        AS Mensaje,
                        n.fecha          AS Fecha,
                        n.leido          AS Leido
                FROM    Notificaciones n
                WHERE   n.usuarioId = @usuarioId
                ORDER BY n.fecha DESC;";

            var result = await db.QueryAsync<NotificacionDto>(sql, new { usuarioId });
            return result.ToList();
        }

        public async Task<bool> MarcarLeidaAsync(int notificacionId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"UPDATE Notificaciones
                        SET leido = 1
                        WHERE notificacionId = @notificacionId;";

            var filas = await db.ExecuteAsync(sql, new { notificacionId });
            return filas > 0;
        }

        public async Task<bool> MarcarTodasLeidasAsync(int usuarioId)
        {
            using var db = new SqlConnection(_connectionString);

            var sql = @"UPDATE Notificaciones
                        SET leido = 1
                        WHERE usuarioId = @usuarioId
                        AND   leido = 0;";

            var filas = await db.ExecuteAsync(sql, new { usuarioId });
            return filas > 0;
        }
    }
}
