using System.Globalization;
using UfoDetector.Models.Enums;

namespace UfoDetector.Converters;

/// <summary>Maps SensorStatus to the corresponding CRT palette colour.</summary>
public class SensorStatusToColorConverter : IValueConverter
{
    // Cached instances — avoids re-parsing hex strings and allocating on every
    // binding update (5 gauges × 10 Hz ticks add up to real per-tick GC pressure).
    private static readonly Color ColourNormal   = Color.FromArgb("#39FF14");
    private static readonly Color ColourElevated = Color.FromArgb("#FFB300");
    private static readonly Color ColourDanger   = Color.FromArgb("#FF2200");

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Normal                         => ColourNormal,
                SensorStatus.Elevated or SensorStatus.Anomaly => ColourElevated,
                _                                           => ColourDanger,
            };
        }
        return ColourNormal;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
