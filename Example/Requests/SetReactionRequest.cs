using System.Text;
using System.Text.Json;
using Telegram.Bot.Requests.Abstractions;

namespace Example.Requests;

internal class SetReactionRequest(long chatId, int messageId, string emoji = "\uD83D\uDC4D") : IRequest<bool>
{
    public HttpMethod Method => HttpMethod.Post;
    public string MethodName => "setMessageReaction";
    public bool IsWebhookResponse { get; set; }

    public HttpContent ToHttpContent()
    {
        var body = new Dictionary<string, object>
        {
            ["chat_id"] = chatId,
            ["message_id"] = messageId,
            ["reaction"] = new Dictionary<string, string>[]
            {
                new()
                {
                    ["type"] = "emoji",
                    ["emoji"] = emoji
                }
            }
        };
        var requestBody = JsonSerializer.Serialize(body);

        return new StringContent(requestBody, Encoding.UTF8, "application/json");
    }
}