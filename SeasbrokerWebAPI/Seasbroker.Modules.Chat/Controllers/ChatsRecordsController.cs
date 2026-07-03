using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Queries;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Controllers;

[ApiController]
[Authorize(Policy = ChatConstants.SuperuserPolicy)]
[Route("api/collections/chats/records")]
public class ChatsRecordsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsRecordsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PocketBaseListResponse<ChatRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var items = await _chatService.GetAllAsync(new GetChatsQuery(), cancellationToken);

        return Ok(new PocketBaseListResponse<ChatRecordDto>
        {
            Page = 1,
            PerPage = items.Count,
            TotalItems = items.Count,
            TotalPages = 1,
            Items = items,
        });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChatRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> View(string id, CancellationToken cancellationToken)
    {
        var chat = await _chatService.GetByIdAsync(new GetChatByIdQuery(id), cancellationToken);
        return Ok(chat);
    }
}
