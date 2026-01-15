namespace Cortex.Core.Models;

public class QuizQuestion
{
    public string Question { get; set; } = string.Empty;
    public string[] Options { get; set; } = Array.Empty<string>();
    public int CorrectAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
}
