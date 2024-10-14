using BasculasPG.Handlers;
using BasculasPG.Models.Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasculasPG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly DataHandler _dataHandler;

        public DataController(DataHandler dataHandler)
        {
            _dataHandler = dataHandler;
        }


        [HttpGet("getBasculasByBod")]
        public async Task<IActionResult> GetBasculasByBod(string bodega_value)
        {
            var medicos = await _dataHandler.getBasculasByBod(bodega_value);
            return Ok(medicos);
        }

        [HttpGet("getKeysGuia")]
        public async Task<IActionResult> GetKeysGuia(string guia, string bodega)
        {
            var result = await _dataHandler.GetKeysGuia(guia, bodega);
            return result.success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getGuiaXManifiesto")]
        public async Task<IActionResult> GetGuiaXManifiesto(string man_anio, string man_corr, string tipoguia_cod)
        {
            var result = await _dataHandler.GetGuiaXManifiesto(man_anio,man_corr,tipoguia_cod);
            return result.success ? Ok(result) : BadRequest(result);
        }
        

        [HttpGet("getPesos")]
        public async Task<IActionResult> GetPesos(int corr, int anio, string tipo, int corelativoPeso)
        {
            var result = await _dataHandler.GetPesos(corr, anio, tipo, corelativoPeso);
            return result.success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("PostPeso")]
        public async Task<IActionResult> PostPeso(PesoRequest guiaRequest)
        {
            var result = await _dataHandler.PostPeso(guiaRequest);
            return result.success ? Ok(result) : BadRequest(result);
        }

    }
}
