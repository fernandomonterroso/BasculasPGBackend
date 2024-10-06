using BasculasPG.DataAccess;
using BasculasPG.Models.Gafete;
using BasculasPG.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BasculasPG.Handlers
{
    public class DataHandler
    {
        private readonly MySqlDbManager _dbManager;
        private readonly IConfiguration _configuration;

        public DataHandler(MySqlDbManager dbManager, IConfiguration configuration)
        {
            _dbManager = dbManager;
            _configuration = configuration;
        }

        public async Task<RespuestaHttp> getBasculasByBod(string bodega_value)
        {
            try
            {
                var parametersQuery = new
                {
                    bodega_value
                };
                IEnumerable<dynamic> result =await _dbManager.DapperExecuteQuery<dynamic>(@"SELECT * FROM combex.cbx_bascula WHERE bas_bodega=@bodega_value", parametersQuery);
                return new RespuestaHttp(true, "EXITO", result);
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }
        }

    }
}
