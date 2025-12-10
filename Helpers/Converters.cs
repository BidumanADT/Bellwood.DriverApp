using System.Globalization;

namespace Bellwood.DriverApp.Helpers;

/// <summary>
/// Converts a boolean value to its inverse
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return !boolValue;
        
        return false;
    }
}

/// <summary>
/// Checks if a value is not null and not empty string
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue)
            return !string.IsNullOrWhiteSpace(stringValue);
        
        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts tracking status booleans to appropriate background color
/// </summary>
public class TrackingStatusToColorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is bool isTracking && values[1] is bool hasError)
        {
            if (hasError)
                return Application.Current?.Resources["Warning"] ?? Colors.Orange;
            if (isTracking)
                return Application.Current?.Resources["Success"] ?? Colors.Green;
        }
        
        return Application.Current?.Resources["Gray400"] ?? Colors.Gray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts bool to GPS icon emoji
/// </summary>
public class BoolToGpsIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTracking && isTracking)
            return "??"; // Active GPS icon
        
        return "??"; // Inactive/searching icon
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
