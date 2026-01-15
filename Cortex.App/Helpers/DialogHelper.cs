using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System.Threading.Tasks;

namespace Cortex.App.Helpers;

/// <summary>
/// Helper class for showing dialogs to users
/// Uses Avalonia's built-in dialog system
/// </summary>
public static class DialogHelper
{
    /// <summary>
    /// Show a prompt dialog to get text input from the user
    /// </summary>
    public static async Task<string?> PromptAsync(Window parent, string title, string message, string? defaultValue = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        var textBox = new TextBox
        {
            Text = defaultValue ?? string.Empty,
            Watermark = "Enter text...",
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };

        var stackPanel = new StackPanel
        {
            Spacing = 10
        };
        stackPanel.Children.Add(new TextBlock { Text = message });
        stackPanel.Children.Add(textBox);

        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync(parent);
        if (result == ContentDialogResult.Primary)
        {
            return textBox.Text;
        }

        return null;
    }

    /// <summary>
    /// Show a confirmation dialog (Yes/No/Cancel)
    /// Returns: true = Yes, false = No, null = Cancel
    /// </summary>
    public static async Task<bool?> ConfirmAsync(Window parent, string title, string message, bool showCancel = false)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            SecondaryButtonText = showCancel ? "Cancel" : "No",
            DefaultButton = ContentDialogButton.Secondary
        };

        if (showCancel)
        {
            dialog.CloseButtonText = "No";
        }

        var result = await dialog.ShowAsync(parent);
        
        if (result == ContentDialogResult.Primary)
            return true;
        if (result == ContentDialogResult.Secondary)
            return showCancel ? null : false;
        if (result == ContentDialogResult.None && showCancel)
            return false;

        return null;
    }

    /// <summary>
    /// Show a simple message dialog
    /// </summary>
    public static async Task ShowMessageAsync(Window parent, string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary
        };

        await dialog.ShowAsync(parent);
    }
}
