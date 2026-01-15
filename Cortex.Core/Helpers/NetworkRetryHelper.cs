using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Cortex.Core.Helpers;

public static class NetworkRetryHelper
{
    public static async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> operation,
        int maxRetries = 3,
        int initialDelayMs = 1000,
        int maxDelayMs = 10000,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await operation(cancellationToken).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                
                // If it's a client error (4xx), don't retry
                if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                {
                    return response;
                }
                
                // For server errors (5xx) or network issues, retry
                if (attempt < maxRetries)
                {
                    var delay = Math.Min(initialDelayMs * (int)Math.Pow(2, attempt), maxDelayMs);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < maxRetries)
                {
                    var delay = Math.Min(initialDelayMs * (int)Math.Pow(2, attempt), maxDelayMs);
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                throw;
            }
        }
        
        if (lastException != null)
        {
            throw lastException;
        }
        
        throw new HttpRequestException("Network request failed after retries");
    }
}
