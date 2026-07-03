using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Seasbroker.Modules.Chat.Application.Commands;
using Seasbroker.Modules.Chat.Application.Constants;
using Seasbroker.Modules.Chat.Application.Services;

namespace Seasbroker.Modules.Chat.Controllers;

[ApiController]
[Route("api/get-chat-token")]
public class ChatTokenController : ControllerBase
{
    private readonly IChatTokenService _chatTokenService;

    public ChatTokenController(IChatTokenService chatTokenService)
    {
        _chatTokenService = chatTokenService;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Issue(CancellationToken cancellationToken)
    {
        var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var response = await _chatTokenService.IssueAsync(
            new IssueChatTokenCommand(remoteIp),
            cancellationToken);

        Response.Cookies.Append(
            ChatConstants.ChatTokenCookieName,
            response.Token,
            new CookieOptions
            {
                HttpOnly = true,
                MaxAge = TimeSpan.FromSeconds(ChatConstants.ChatTokenCookieMaxAgeSeconds),
            });

        return Ok(response);
    }
}
