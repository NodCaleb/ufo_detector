using System.Globalization;
using UfoDetector.Models.Enums;

namespace UfoDetector.Converters;

/// <summary>Maps SensorStatus to the corresponding CRT palette colour.</summary>
public class SensorStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Normal                         => Color.FromArgb("#39FF14"),
                SensorStatus.Elevated or SensorStatus.Anomaly => Color.FromArgb("#FFB300"),
                _                                           => Color.FromArgb("#FF2200"),
            };
        }
        return Color.FromArgb("#39FF14");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
