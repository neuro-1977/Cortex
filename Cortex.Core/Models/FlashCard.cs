namespace Cortex.Core.Models;

public class FlashCard
{
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public bool IsFlipped { get; set; } = false;
    public string? Explanation { get; set; }
}
