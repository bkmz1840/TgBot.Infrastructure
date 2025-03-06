namespace Example.Contexts;

internal record ExampleHandlerContext
{
    public string? CurrentText { get; set; }
    public string? LastText { get; set; }
    public int? LastMessageId { get; set; }
    public bool LastMessageLiked { get; set; }
}