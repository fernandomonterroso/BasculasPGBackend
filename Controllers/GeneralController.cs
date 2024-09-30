using BasculasPG.Handlers;
using BasculasPG.Models.Gafete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasculasPG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        private readonly GeneralHandler _generalHandler;

        public GeneralController(GeneralHandler generalHandler)
        {
            _generalHandler = generalHandler;
        }
        [Authorize]
        [HttpGet("getService")]
        public async Task<IActionResult> getService()
        {
            var medicos = await _generalHandler.getService();
            return Ok(medicos);
        }

        
        [HttpPost("authGafete")]
        public async Task<IActionResult> authGafete(LoginGafete gafete)
        {
            var respuesta = await _generalHandler.authGafete(gafete);
            return Ok(respuesta);
        }
    }
}
