using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.LLM;

public interface ILlmClient
{
    Task<string> GenerateAsync(LlmRequest request, CancellationToken cancellationToken = default);
}

