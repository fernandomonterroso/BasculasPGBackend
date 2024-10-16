using BasculasPG.DataAccess;
using BasculasPG.Models;
using BasculasPG.Models.Gafete;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BasculasPG.Handlers
{
    public class GeneralHandler
    {
        private readonly MySqlDbManager _dbManager;
        private readonly IConfiguration _configuration;

        public GeneralHandler(MySqlDbManager dbManager, IConfiguration configuration)
        {
            _dbManager = dbManager;
            _configuration = configuration;
        }

        public async Task<RespuestaHttp> getService()
        {
            return new RespuestaHttp(true, "SERVICIO ACTIVO");
        }

        public async Task<RespuestaHttp> authGafete(LoginGafete gafete)
        {
            try
            {
                var result = _dbManager.DapperExecuteQuery<dynamic>(
                    @"SELECT * FROM sab_regi_user WHERE USER_GAFETE=@USER_GAFETE",
                    new { USER_GAFETE = gafete.gafete }).Result.FirstOrDefault();

                _ = EnviarCorreo("elmerfer_mejor@hotmail.com", "", "Nueva solicitud de ingreso de persona:" + gafete.gafete, "prueba");

                if (result == null) return new RespuestaHttp(false, "Usuario no encontrado");

                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                new Claim(ClaimTypes.Name, result.USER_USUARIO),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("CustomKey", result.USER_ID.ToString())
            }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["JwtSettings:Issuer"],
                    Audience = _configuration["JwtSettings:Audience"]
                };
                var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

                
                return new RespuestaHttp(true, "EXITO", new { data = result, token });
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }
        }

        private async Task EnviarCorreo(string to, string cc, string asunto, string html)
        {
            var httpClient = new HttpClient();
            var data = new
            {
                correo = new { to, cc, asunto, type = "8" },
                html
            };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            try { await httpClient.PostAsync("https://www.combexim.com.gt/consultas/module/automate/ApiController.php?FUNC=enviarCorreo", content); }
            catch (Exception) { /* Manejo de errores opcional */ }
        }

    }


}

