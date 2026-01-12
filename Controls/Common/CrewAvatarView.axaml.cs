using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Serenity.Diagnostics;
using Serenity.ViewModels;
using System;
using System.IO;

namespace Serenity.Controls;

public sealed partial class CrewAvatarView : UserControl
{
    public CrewAvatarView()
    {
        InitializeComponent();

        AttachedToVisualTree += (_, _) => StartOrUpdate();
        DataContextChanged += (_, _) => StartOrUpdate();
    }

    private void StartOrUpdate()
    {
        try
        {
            if (DataContext is not MainViewModel.CrewMember member)
            {
                ClearImage();
                return;
            }

            // Image-only avatar (stable for published builds).
            // Prefer the crew profile image; if missing, leave blank rather than crashing.
            ShowImage(member.ProfileImagePath);
        }
        catch (Exception ex)
        {
            CrashLog.Write("CrewAvatarView.StartOrUpdate", ex);
        }
    }

    private void ShowImage(string? imagePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                ClearImage();
                return;
            }

            using var fs = File.OpenRead(imagePath);
            AvatarImage.Source = new Bitmap(fs);
        }
        catch
        {
            ClearImage();
        }
    }

    private void ClearImage()
    {
        try
        {
            AvatarImage.Source = null;
        }
        catch
        {
            // Ignore.
        }
    }
}
