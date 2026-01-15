using System;

namespace Cortex.App.Helpers;

public static class ErrorHandlingService
{
    public static void HandleException(Exception ex, string context)
    {
        System.Diagnostics.Debug.WriteLine($"[{context}] Error: {ex.Message}");
        System.Diagnostics.Debug.WriteLine($"[{context}] Stack trace: {ex.StackTrace}");
    }
}
