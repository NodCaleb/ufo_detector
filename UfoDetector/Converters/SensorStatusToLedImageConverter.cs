using System.Globalization;
using UfoDetector.Models.Enums;

namespace UfoDetector.Converters;

/// <summary>
/// Maps SensorStatus to one of a pair of LED indicator images.
/// ConverterParameter "Left" lights yellow at Anomaly and above; "Right" lights red at Danger and above.
/// </summary>
public class SensorStatusToLedImageConverter : IValueConverter
{
    private const string LedOff    = "led_off.png";
    private const string LedYellow = "led_yellow.png";
    private const string LedRed    = "led_red.png";

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not SensorStatus status)
            return LedOff;

        bool isRight = string.Equals(parameter as string, "Right", StringComparison.OrdinalIgnoreCase);

        if (isRight)
            return status is SensorStatus.Danger or SensorStatus.Critical or SensorStatus.CriticalHigh
                ? LedRed
                : LedOff;

        return status switch
        {
            SensorStatus.Anomaly or SensorStatus.Danger or SensorStatus.Critical or SensorStatus.CriticalHigh
                => LedYellow,
            _ => LedOff,
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
