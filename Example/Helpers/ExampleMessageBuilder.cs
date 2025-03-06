using Example.Contexts;
using System.Text;

namespace Example.Helpers;

internal class ExampleMessageBuilder
{
    public string Build(ExampleHandlerContext context)
    {
        var builder = new StringBuilder();

        builder.Append($"You send me: {context.CurrentText}\n\n");
        builder.Append($"Last was: {context.LastText ?? "nothing"}\n");
        builder.Append("Last message liked: ");
        builder.Append(context.LastMessageLiked ? "Yep" : "No");

        return builder.ToString();
    }
}
