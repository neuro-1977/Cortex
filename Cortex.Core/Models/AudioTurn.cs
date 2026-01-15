namespace Cortex.Core.Models;

public class AudioTurn
{
    public string Speaker { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? AudioPath { get; set; }
    public bool IsPlaying { get; set; } = false;
    public double? Duration { get; set; }
}
