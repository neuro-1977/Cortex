namespace Cortex.Core.LLM;

public sealed record LlmRequest(
    string Prompt,
    string? Model = null,
    double Temperature = 0.2,
    string? SystemPrompt = null
);

