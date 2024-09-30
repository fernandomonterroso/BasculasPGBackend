using BasculasPG.DataAccess;
using BasculasPG.Models;
using BasculasPG.Models.Gafete;
using Microsoft.IdentityModel.Tokens;
using MySqlX.XDevAPI.Common;
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
                    @"SELECT * FROM SAB_REGI_USER WHERE USER_GAFETE=@USER_GAFETE",
                    new { USER_GAFETE = gafete.gafete }).Result.FirstOrDefault();

                if (result == null) return new RespuestaHttp(false, "Usuario no encontrado");

                var key = Encoding.ASCII.GetBytes(_configuration["JwtSettings:SecretKey"]);
                var tokenHandler = new JwtSecurityTokenHandler();

                // Crear el descriptor del token y agregar la clave personalizada
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.Name, result.USER_USUARIO),  // Reclamo de nombre de usuario
                    new Claim(ClaimTypes.Role, "User"),               // Reclamo de rol
                    new Claim("CustomKey", result.USER_ID.ToString())  // Reclamo personalizado
                }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _configuration["JwtSettings:Issuer"],
                    Audience = _configuration["JwtSettings:Audience"]
                };

                var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

                // Retornar la respuesta con los datos y el token
                return new RespuestaHttp(true, "EXITO", new { data = result, token });
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }

            //var handler = new JwtSecurityTokenHandler();
            //var jwtToken = handler.ReadJwtToken(tokenString);

            //// Obtener el valor de la clave personalizada "CustomKey"
            //var customKey = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "CustomKey")?.Value
        }

    }


}

