using System;
using System.Globalization;
using System.Windows.Data;

namespace SunoMetatagApp;

public sealed class CountGreaterThanOneToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int n && n > 1;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
