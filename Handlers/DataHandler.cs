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
                IEnumerable<dynamic> result = await _dbManager.DapperExecuteQuery<dynamic>(@"SELECT * FROM combex.cbx_bascula WHERE bas_bodega=@bodega_value", parametersQuery);
                return new RespuestaHttp(true, "EXITO", result);
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }
        }

        public async Task<RespuestaHttp> GetKeysGuia(string guia, string bodega)
        {
            try
            {
                throw new Exception("Error intencional para probar el manejo de errores.");
                string pre = guia.Substring(0, 3);
                string num = guia.Substring(3);

                string strConsulta = $@"
                SELECT 
                    GUIA_PREFIJO || guia_num AS GUIA,
                    GUIA_ANIO,
                    GUIA_CORR,
                    TIPOGUIA_COD,
                    GUIA_CONSIGNATARIO,
                    GUIA_PIEZA,
                    GUIA_PESO,
                    GUIA_PESOKG,
                    GUIA_REPESOKG,
                    ESTADO_COD,
                    GUIA_ORDEN,
                    GUIA_FECHORING AS fecha,
                    IFNULL(GUIA_PIEZACF, 0) AS GUIA_PIEZACF
                FROM cbx_guia
                WHERE CIA_COD = 'ASO'
                AND GUIA_PREFIJO = @pre
                AND guia_num = @num
                AND TIPOGUIA_COD = @bodega";

                var parametersQuery = new { pre, num, bodega };
                var result = (await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery)).FirstOrDefault();

                return new RespuestaHttp(true, "EXITO", result);
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }
        }


        private async Task<IEnumerable<dynamic>> GetPesosGuia(int corr, int anio, string tipo)
        {
            try
            {
                string strConsulta = $@"
            SELECT 
                CASE Peso_Usobascula WHEN 'S' THEN 'true' ELSE 'false' END AS Peso_Usobascula,
                Bas_cod,
                CASE Peso_Cfrio WHEN 'S' THEN 'true' ELSE 'false' END AS Peso_Cfrio,
                Tembalaje_Cod,
                Peso_Pesobrutokg,
                Peso_Pesobruto,
                DATE_FORMAT(Peso_Fechor, '%d/%m/%Y %H:%i:%s') AS Peso_Fechor,
                Peso_Aniocod,  
                Peso_Codigo,
                peso_corr,
                Peso_Correquipo,
                Guia_Corr
            FROM cbx_peso
            WHERE
                cia_cod = 'ASO' 
                AND peso_corr = (
                    SELECT IFNULL(MAX(PESO_CORR), 0)
                    FROM cbx_peso
                    WHERE 
                        Cia_Cod = 'ASO'  
                        AND Guia_Corr = @corr  
                        AND Guia_Anio = @anio
                        AND Tipoguia_Cod = @tipo
                )
                AND Guia_Corr = @corr
                AND Guia_Anio = @anio
                AND Tipoguia_Cod = @tipo
            ORDER BY Peso_Correquipo";

                var parametersQuery = new { corr, anio, tipo };
                return await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task<dynamic> GetMaxItemPeso(int corr, int anio, string tipo, int getMaxItemPeso)
        {
            try
            {
                string strConsulta = $@"
            SELECT IFNULL(MAX(PESO_CORR), 0) AS CORRELATIVO, @getMaxItemPeso AS PARAMETRO, 0 PESO_CODIGO
            FROM cbx_peso
            WHERE 
                Cia_Cod = 'ASO'  
                AND Guia_Corr = @corr  
                AND Guia_Anio = @anio
                AND Tipoguia_Cod = @tipo";

                var parametersQuery = new { corr, anio, tipo, getMaxItemPeso };
                var result = (await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery)).FirstOrDefault();

                if (result == null) return null;

                var idSol = await GetId(corr, anio, tipo, getMaxItemPeso);
                result.PESO_CODIGO = idSol.ToString();

                string strAnio = "SELECT YEAR(CURRENT_DATE()) AS ANIO";
                var arrConsultaAnio = (await _dbManager.DapperExecuteQuery<dynamic>(strAnio, null)).FirstOrDefault();
                result.PESO_ANIOCOD = arrConsultaAnio.ANIO;

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        private async Task<int> GetId(int guia_corr, int guia_anio, string tipoguia_cod, int peso_corr)
        {
            try
            {
                // Consulta para obtener el máximo PESO_CODIGO
                string strConsulta = $@"
                SELECT IFNULL(MAX(PESO_CODIGO), 0) AS RESULT
                FROM cbx_peso
                WHERE PESO_CORR = @peso_corr AND Guia_Corr = @guia_corr 
                      AND Guia_Anio = @guia_anio AND Tipoguia_Cod = @tipoguia_cod";

                var parametersQuery = new { peso_corr, guia_corr, guia_anio, tipoguia_cod };
                var arrConsulta = await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery);

                int result = (int)(arrConsulta.FirstOrDefault()?.RESULT ?? 0L); // Convertir a int

                if (result == 0)
                {
                    // Consulta para obtener el siguiente PESO_CODIGO
                    strConsulta = @"
                        SELECT IFNULL(MAX(PESO_CODIGO), 0) + 1 AS RESULT
                        FROM cbx_peso
                        WHERE PESO_ANIOCOD = YEAR(CURRENT_DATE())";

                    arrConsulta = await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, null);
                    result = (int)(arrConsulta.FirstOrDefault()?.RESULT ?? 0L); // Convertir a int
                }
                else
                {
                    result += 1; // Incrementar el resultado
                }

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private async Task<IEnumerable<dynamic>> GetPesosCorrGuia(int corr, int anio, string tipo, int corelativoPeso)
        {
            try
            {
                string strConsulta = $@"
            SELECT 
                CASE Peso_Usobascula WHEN 'S' THEN 'true' ELSE 'false' END AS Peso_Usobascula,
                Bas_cod,
                CASE Peso_Cfrio WHEN 'S' THEN 'true' ELSE 'false' END AS Peso_Cfrio,
                Tembalaje_Cod,
                Peso_Pesobrutokg,
                Peso_Pesobruto,
                DATE_FORMAT(Peso_Fechor, '%d/%m/%Y %H:%i:%s') AS Peso_Fechor,
                Peso_Aniocod,  
                Peso_Codigo,
                peso_corr,
                Peso_Correquipo,
                Guia_Corr
            FROM cbx_peso
            WHERE
                cia_cod = 'ASO' 
                AND peso_corr = @corelativoPeso
                AND Guia_Corr = @corr
                AND Guia_Anio = @anio
                AND Tipoguia_Cod = @tipo
            ORDER BY Peso_Correquipo";

                var parametersQuery = new { corr, anio, tipo, corelativoPeso };
                return await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<RespuestaHttp> GetPesos(int corr, int anio, string tipo, int corelativoPeso)
        {
            try
            {
                var data = corelativoPeso == 0
                    ? new
                    {
                        data = await GetPesosGuia(corr, anio, tipo),
                        informacion = await GetMaxItemPeso(corr, anio, tipo, corelativoPeso)
                    }
                    : new
                    {
                        data = await GetPesosCorrGuia(corr, anio, tipo, corelativoPeso),
                        informacion = await GetMaxItemPeso(corr, anio, tipo, corelativoPeso)
                    };

                return new RespuestaHttp(true, "EXITO", data);
            }
            catch (Exception e)
            {
                return new RespuestaHttp(false, e.Message);
            }
        }

    }
}
