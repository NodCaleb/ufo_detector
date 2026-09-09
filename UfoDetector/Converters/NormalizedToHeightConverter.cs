using System.Globalization;

namespace UfoDetector.Converters;

/// <summary>Scales a normalized 0–1 value to a pixel height, given the full-scale height as ConverterParameter.</summary>
public class NormalizedToHeightConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double normalized && parameter is string heightText &&
            double.TryParse(heightText, NumberStyles.Float, CultureInfo.InvariantCulture, out var fullHeight))
        {
            return System.Math.Clamp(normalized, 0d, 1d) * fullHeight;
        }
        return 0d;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
