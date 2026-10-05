using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Controllers;

public class VisitorChatHistoryRequest
{
    [JsonPropertyName("chatId")]
    public string? ChatId { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}

/// <summary>
/// A website visitor reading their own conversation (including our replies sent while they were away).
/// </summary>
[ApiController]
[Tags("Chat")]
[Route("api/chat")]
public class VisitorChatController : ControllerBase
{
    private readonly IMessageService _messageService;

    public VisitorChatController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    /// <summary>
    /// Returns the conversation for the chat the token was issued for. A POST so the token travels in
    /// the body rather than in a URL that ends up in access logs.
    /// </summary>
    [HttpPost("history")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PocketBaseListResponse<MessageRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> History([FromBody] VisitorChatHistoryRequest request, CancellationToken cancellationToken)
    {
        var items = await _messageService.GetForVisitorAsync(request.ChatId, request.Token, cancellationToken);
        return Ok(new PocketBaseListResponse<MessageRecordDto>
        {
            Page = 1,
            PerPage = items.Count,
            TotalItems = items.Count,
            TotalPages = 1,
            Items = items,
        });
    }
}
