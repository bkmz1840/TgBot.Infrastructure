using Telegram.Bot.Types.ReplyMarkups;

namespace TgBot.Infrastructure.Helpers;

public class InlineKeyboardMarkupBuilder
{
    private readonly List<List<InlineKeyboardButton>> buttonRows = [];
    
    public int CurrentButtonRowIndex { get; private set; }
    public int ButtonRowsCount => buttonRows.Count;

    public InlineKeyboardMarkupBuilder AddButtonRow()
    {
        buttonRows.Add([]);
        CurrentButtonRowIndex = buttonRows.Count - 1;
        
        return this;
    }

    public InlineKeyboardMarkupBuilder AddButton(InlineKeyboardButton button)
    {
        if (CurrentButtonRowIndex >= buttonRows.Count)
        {
            throw new InvalidOperationException("Cannot add button to non-existent row");
        }
        
        buttonRows[CurrentButtonRowIndex].Add(button);
        return this;
    }

    public InlineKeyboardMarkupBuilder MoveToNextRow()
    {
        if (CurrentButtonRowIndex == ButtonRowsCount - 1)
        {
            throw new InvalidOperationException("Cannot move to next row. Current index is last");
        }
        
        CurrentButtonRowIndex++;
        return this;
    }

    public InlineKeyboardMarkupBuilder MoveToPreviousRow()
    {
        if (CurrentButtonRowIndex == 0)
        {
            throw new InvalidOperationException("Cannot move to previous row. Current index is first");
        }
        
        CurrentButtonRowIndex--;
        return this;
    }

    public IReplyMarkup Build() => new InlineKeyboardMarkup(buttonRows);
}