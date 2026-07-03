using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.DTOs;
using Seasbroker.Modules.Chat.Application.Extensions;
using Seasbroker.Modules.Chat.Application.Helpers;
using Seasbroker.Modules.Chat.Application.Queries;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Controllers;

[ApiController]
[Route("api/collections/messages/records")]
public class MessagesRecordsController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesRecordsController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet]
    [Authorize(Policy = ChatConstants.SuperuserPolicy)]
    [ProducesResponseType(typeof(PocketBaseListResponse<MessageRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] string? filter,
        [FromQuery] string? sort,
        CancellationToken cancellationToken)
    {
        var chatId = PocketBaseFilterParser.TryParseChatIdEquals(filter) ?? string.Empty;
        var items = await _messageService.GetByChatIdAsync(
            new GetMessagesByChatIdQuery(chatId, sort ?? "created"),
            cancellationToken);

        return Ok(new PocketBaseListResponse<MessageRecordDto>
        {
            Page = 1,
            PerPage = items.Count,
            TotalItems = items.Count,
            TotalPages = 1,
            Items = items,
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MessageRecordDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        if (User.IsSuperuser())
        {
            var adminRequest = await JsonSerializer.DeserializeAsync<CreateAdminMessageRequest>(
                Request.Body,
                cancellationToken: cancellationToken);

            if (adminRequest is null ||
                string.IsNullOrWhiteSpace(adminRequest.ChatId) ||
                string.IsNullOrWhiteSpace(adminRequest.Content))
            {
                return BadRequest(new PocketBaseErrorResponse
                {
                    Message = "Bad call to create message",
                    Status = StatusCodes.Status400BadRequest,
                });
            }

            var adminMessage = await _messageService.CreateAsAdminAsync(
                new CreateAdminMessageCommand(adminRequest.ChatId, adminRequest.Content),
                cancellationToken);

            return Ok(adminMessage);
        }

        var anonymousRequest = await JsonSerializer.DeserializeAsync<CreateAnonymousMessageRequest>(
            Request.Body,
            cancellationToken: cancellationToken);

        if (anonymousRequest is null)
        {
            return BadRequest(new PocketBaseErrorResponse
            {
                Message = "Bad call to create message",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var anonymousMessage = await _messageService.CreateAsAnonymousAsync(
            new CreateAnonymousMessageCommand(
                anonymousRequest.Token,
                anonymousRequest.ChatId,
                anonymousRequest.Content),
            cancellationToken);

        return Ok(anonymousMessage);
    }
}
