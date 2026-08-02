using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers
{
    [Route("api/GetAllowedPriorities")]
    [ApiController]
     [Tags("ENUMs")]
    public class GetAllowedPrioritiesController : ControllerBase
    {
        /// <summary>
        /// Lista as prioridades permitidas para as tarefas.
        /// </summary>
        /// <remarks>
        /// Retorna a lista oficial de valores aceitos pelo sistema para o atributo priority:
        /// - **Baixo**
        /// - **Médio-Baixo**
        /// - **Médio**
        /// - **Alto**
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult Handle()
        {
            var allowedPriorities = new[]
            {
                "Baixo",
                "Médio-Baixo",
                "Médio",
                "Alto"
            };

            return Ok(allowedPriorities);
        }
    }
}