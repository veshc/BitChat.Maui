using BitChat.Maui.Core.Models;
using System.Globalization;

namespace BitChat.Maui.Presentation.Converters;

/// <summary>
/// Converts message status to visibility for checkmark display
/// </summary>
public class StatusToCheckmarkConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is MessageStatus status)
        {
            return status == MessageStatus.Sent || status == MessageStatus.Delivered || status == MessageStatus.Read;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}