using System.Text.Json.Serialization;

namespace Seasbroker.Modules.Chat.Application.DTOs;

public class CreateAnonymousMessageRequest
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("chatId")]
    public string ChatId { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
