using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cortex.App.Helpers;

public static class PerformanceProfiler
{
    public static async Task MeasureAsync(string operationName, Func<Task> operation)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            await operation();
        }
        finally
        {
            sw.Stop();
            Debug.WriteLine($"[Performance] {operationName} took {sw.ElapsedMilliseconds}ms");
        }
    }
}
