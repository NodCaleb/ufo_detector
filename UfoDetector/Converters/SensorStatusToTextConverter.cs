using System.Globalization;
using UfoDetector.Models.Enums;

namespace UfoDetector.Converters;

/// <summary>Maps SensorStatus to its Russian display string.</summary>
public class SensorStatusToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SensorStatus status)
        {
            return status switch
            {
                SensorStatus.Normal      => "НОРМА",
                SensorStatus.Elevated    => "ПОВЫШЕН",
                SensorStatus.Anomaly     => "АНОМАЛИЯ",
                SensorStatus.Danger      => "ОПАСНОСТЬ",
                SensorStatus.Critical    => "КРИТИЧНО",
                SensorStatus.CriticalHigh=> "КРИТИЧНО",
                _                        => "НОРМА",
            };
        }
        return "НОРМА";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
