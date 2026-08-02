using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers
{
    [Route("api/GetAllowedStatuses")]
    [ApiController]
    [Tags("ENUMs")]
    public class GetAllowedStatusesController : ControllerBase
    {
        /// <summary>
        /// Lista os status permitidos para as tarefas.
        /// </summary>
        /// <remarks>
        /// Retorna a lista oficial de valores aceitos pelo sistema para o atributo status:
        /// - **A Fazer**
        /// - **Em Andamento**
        /// - **Concluído**
        /// - **Excluído**
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult Handle()
        {
            var allowedStatuses = new[]
            {
                "A Fazer",
                "Em Andamento",
                "Concluído",
                "Excluído"
            };

            return Ok(allowedStatuses);
        }
    }
}