using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Cortex.App.Converters;

/// <summary>
/// Converter to check if an artifact is currently generating media
/// </summary>
public class IsArtifactGeneratingMediaConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // This converter checks if the artifact ID matches the one currently generating
        // Parameter should be the ViewModel's IsGeneratingMedia and GeneratingArtifactId
        if (parameter is Cortex.App.ViewModels.CortexViewModel vm)
        {
            if (value is string artifactId)
            {
                return vm.IsGeneratingMedia && vm.GeneratingArtifactId == artifactId;
            }
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
