using Microsoft.AspNetCore.Mvc;

namespace TodoApi.Controllers;

[Route("api/GetAuditActions")]
[ApiController]
[Tags("ENUMs")]
public class GetAuditActionsController : ControllerBase
{
    public class AuditActionDto
    {
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// </summary>
    /// <remarks>
    /// Retorna a lista de chaves de auditoria (Actions) que são salvas no banco, 
    /// junto com a descrição detalhada do evento que dispara cada uma delas.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AuditActionDto>), StatusCodes.Status200OK)]
    public IActionResult Handle()
    {
        var auditActions = new List<AuditActionDto>
        {
            new AuditActionDto 
            { 
                Action = "USER_LOGIN", 
                Description = "Registrado quando um usuário realiza o login com sucesso no sistema." 
            },
            new AuditActionDto 
            { 
                Action = "REGISTER_USER", 
                Description = "Registrado quando um usuário externo realiza o auto cadastro (cria a própria conta)." 
            },
            new AuditActionDto 
            { 
                Action = "ADMIN_CREATE_USER", 
                Description = "Registrado quando um Administrador cria a conta de um novo usuário através da rota restrita." 
            },
            new AuditActionDto 
            { 
                Action = "PROMOTE_USER", 
                Description = "Registrado quando um Administrador concede a permissão de 'Admin' para outro usuário." 
            },
            new AuditActionDto 
            { 
                Action = "CHANGE_PASSWORD", 
                Description = "Registrado quando um usuário (seja Admin ou Comum) altera a sua própria senha." 
            },
            new AuditActionDto 
            { 
                Action = "CREATE_TASK", 
                Description = "Registrado quando uma nova tarefa é adicionada ao sistema." 
            },
            new AuditActionDto 
            { 
                Action = "UPDATE_TASK", 
                Description = "Registrado quando as informações de uma tarefa existente (título, descrição, status ou vencimento) são atualizadas." 
            },
            new AuditActionDto 
            { 
                Action = "DELETE_TASK", 
                Description = "Registrado quando uma tarefa é excluída (movida para a lixeira via soft delete)." 
            },
            new AuditActionDto 
            { 
                Action = "SHARE_TASK", 
                Description = "Registrado quando o dono de uma tarefa a compartilha com o e-mail de outro usuário da plataforma." 
            }
        };

        return Ok(auditActions);
    }
}