using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class ArmedToGlyphConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (value is bool b && b) ? "◉ Armed" : "○ Disarmed";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
