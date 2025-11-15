using ASECCC_API.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ASECCC_API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        public HomeController(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost]
        [Route("Registro")]
        public IActionResult Registro(RegistroUsuarioRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@NombreCompleto", usuario.NombreCompleto);
                parametros.Add("@CorreoElectronico", usuario.CorreoElectronico);
                parametros.Add("@Contrasena", usuario.Contrasena);
                parametros.Add("@TipoIdentificacion", usuario.TipoIdentificacion);
                parametros.Add("@Identificacion", usuario.Identificacion);
                parametros.Add("@FechaNacimiento", usuario.FechaNacimiento);
                parametros.Add("@Telefono", usuario.Telefono);
                parametros.Add("@Direccion", usuario.Direccion);

                var resultado = context.Execute("Registro", parametros);
                return Ok(resultado);
            }
        }


        [HttpPost]
        [Route("ValidarSesion")]
        public IActionResult ValidarSesion(ValidarSesionRequestModel usuario)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@Identificacion", usuario.Identificacion);
                parametros.Add("@Contrasena", usuario.Contrasena);

                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ValidarSesion", parametros);

                if (resultado != null)
                {
                    //JWT
                    resultado.Token = GenerarToken(resultado.UsuarioId, resultado.NombreCompleto, resultado.PerfilId);
                    return Ok(resultado);
                }

                return NotFound();
            }
        }


        [HttpGet]
        [Route("ValidarUsuario")]
        public IActionResult ValidarUsuario([Required] string CorreoElectronico)
        {
            using (var context = new SqlConnection(_configuration["ConnectionStrings:BDConnection"]))
            {
                var parametros = new DynamicParameters();
                parametros.Add("@CorreoElectronico", CorreoElectronico);
                var resultado = context.QueryFirstOrDefault<DatosUsuarioResponseModel>("ValidarUsuario", parametros);

                if (resultado != null)
                {
                    //Actualizar Contraseña
                    var ContrasennaGenerada = GenerarContrasenna();

                    var parametrosActualizar = new DynamicParameters();
                    parametrosActualizar.Add("@UsuarioId", resultado.UsuarioId);
                    parametrosActualizar.Add("@Contrasena", ContrasennaGenerada);
                    var resultadoActualizar = context.Execute("ActualizarContrasenna", parametrosActualizar);

                    if (resultadoActualizar > 0)
                    {
                        //Enviar Correo
                        var ruta = Path.Combine(_environment.ContentRootPath, "PlantillaCorreo.html");
                        var html = System.IO.File.ReadAllText(ruta, UTF8Encoding.UTF8);

                        html = html.Replace("{{NombreCompleto}}", resultado.NombreCompleto);
                        html = html.Replace("{{Contrasena}}", ContrasennaGenerada);

                        EnviarCorreo("Recuperar Acceso", html, resultado.CorreoElectronico);
                        return Ok(resultado);
                    }
                }

                return NotFound();
            }
        }

        private string GenerarContrasenna()
        {
            int longitud = 8;
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder resultado = new();

            using var rng = RandomNumberGenerator.Create();
            byte[] buffer = new byte[1];

            while (resultado.Length < longitud)
            {
                rng.GetBytes(buffer);
                int valor = buffer[0] % caracteres.Length;
                resultado.Append(caracteres[valor]);
            }

            return resultado.ToString();
        }

        private void EnviarCorreo(string subject, string body, string destinatario)
        {
            var correoSMTP = _configuration["Valores:CorreoSMTP"]!;
            var contrasennaSMTP = _configuration["Valores:ContrasennaSMTP"]!;

            if (string.IsNullOrEmpty(contrasennaSMTP))
                return;

            var mensaje = new MailMessage
            {
                From = new MailAddress(correoSMTP),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mensaje.To.Add(destinatario);

            using var smtp = new SmtpClient("smtp.office365.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(correoSMTP, contrasennaSMTP),
                EnableSsl = true
            };

            smtp.Send(mensaje);
        }

        private string GenerarToken(int usuarioId, string nombre, int rol)
        {
            var key = _configuration["Valores:KeyJWT"]!;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", usuarioId.ToString()),
                new Claim("nombre", nombre),
                new Claim("rol", rol.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
