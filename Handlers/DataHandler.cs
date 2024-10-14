using BasculasPG.DataAccess;
using BasculasPG.Models.Gafete;
using BasculasPG.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BasculasPG.Models.Dapper;
using MySql.Data.MySqlClient;

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
                string pre = guia.Substring(0, 3);
                string num = guia.Substring(3);

                string strConsulta = $@"
                SELECT 
                    CIA_COD,
                    CONCAT(GUIA_PREFIJO, guia_num) AS GUIA,
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
        public async Task<RespuestaHttp> GetGuiaXManifiesto(string man_anio,string man_corr, string tipoguia_cod)
        {
            try
            {
                string strConsulta = $@"
                SELECT 
                CIA_COD,
                CONCAT(GUIA_PREFIJO, guia_num) AS GUIA,
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
                 FROM combex.cbx_guia
                where
                man_anio=@man_anio and man_corr=@man_corr and tipoguia_cod=@tipoguia_cod
                ";

                var parametersQuery = new { man_anio, man_corr, tipoguia_cod };
                var result = await _dbManager.DapperExecuteQuery<dynamic>(strConsulta, parametersQuery);
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
                PESO_PESOBRUTOLB,
                PESO_NETOKG,
                PESO_TARAKG,
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

                int idSol = await GetId(corr, anio, tipo, getMaxItemPeso);
                result.PESO_CODIGO = (object)idSol;

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
                PESO_PESOBRUTOLB,
                PESO_NETOKG,
                PESO_TARAKG,
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

        public async Task<RespuestaHttp> PostPeso(PesoRequest guiaRequest)
        {
            try
            {
                int correlativoPeso = guiaRequest.InfoPeso.CORRELATIVO;
                int index = 1;
                Boolean guiaPrimerPeso = guiaRequest.InfoPeso.CORRELATIVO>0 ? false : true;

                // Si es repeso, incrementamos el correlativo
                if (guiaRequest.InfoInterna.repeso)
                {
                    correlativoPeso += 1;
                }
                else
                {
                    correlativoPeso = guiaRequest.InfoInterna.currentPage;
                }


                foreach (InsertDetallePeso peso in guiaRequest.pesos)
                {
                    var query = @"
            INSERT INTO combex.cbx_peso (
                CIA_COD,
                GUIA_CORR,
                GUIA_ANIO,
                TIPOGUIA_COD,
                PESO_CORR,
                PESO_CORREQUIPO,
                PESO_TARAKG,
                BAS_COD,
                PESO_PESOBRUTOKG,
                PESO_CANTPIEZA,
                PESO_NETOKG,
                USER_ID,
                PESO_PESOBRUTOLB,   
                BAS_BODEGA,
                PESO_FECHOR,
                PESO_CFRIO,
                PESO_USOBASCULA
            ) VALUES (
                @CIA_COD,
                @GUIA_CORR,
                @GUIA_ANIO,
                @TIPOGUIA_COD,
                @PESO_CORR,
                (SELECT IFNULL(MAX(PESO_CORREQUIPO), 0) + 1  FROM cbx_peso p where p.guia_corr=@GUIA_CORR and p.guia_anio=@GUIA_ANIO and p.tipoguia_cod=@TIPOGUIA_COD and p.peso_corr=@index),
                @PESO_TARAKG,
                @BAS_COD,
                @PESO_PESOBRUTOKG,
                @PESO_CANTPIEZA,
                @PESO_NETOKG,
                @USER_ID,
                @PESO_PESOBRUTOLB,
                @BAS_BODEGA,
                SYSDATE(),
                'N',
                'S'
            );";

                    var parameters = new
                    {
                        guiaRequest.guiaData.CIA_COD,
                        guiaRequest.guiaData.GUIA_CORR,
                        guiaRequest.guiaData.GUIA_ANIO,
                        guiaRequest.guiaData.TIPOGUIA_COD,
                        PESO_CORR = correlativoPeso,
                        index = correlativoPeso,
                        peso.PESO_CORREEQUIPO,
                        peso.PESO_TARAKG,
                        peso.BAS_COD,
                        peso.PESO_PESOBRUTOKG,
                        peso.PESO_CANTPIEZA,
                        peso.PESO_NETOKG,
                        peso.USER_ID,
                        peso.PESO_PESOBRUTOLB,
                        peso.BAS_BODEGA
                    };

                    await _dbManager.DapperExecuteCommand(query, parameters);
                    var queryGuia = @"
                    update cbx_guia set GUIA_PESOKG=
                    AND Guia_Corr = @corr
                    AND Guia_Anio = @anio
                    AND Tipoguia_Cod = @tipo";
                    //await _dbManager.DapperExecuteCommand(query, parameters);
                }

                return new RespuestaHttp(true, "EXITO");
            }
            catch (MySqlException ex)
            {
                return new RespuestaHttp(false, $"Error En base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new RespuestaHttp(false, ex.Message);
            }
        }


    }
}
