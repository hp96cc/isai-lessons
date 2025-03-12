using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace ISAI.Lessons.Mobile.Convertor
{
    public class BoolToButtonColourConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(bool)value)
            {
                return Color.FromArgb("#18b8ff");
            } else
            {
                return Colors.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;

        }
    }
}
