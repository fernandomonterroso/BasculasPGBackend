using BasculasPG.Handlers;
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
    }
}
