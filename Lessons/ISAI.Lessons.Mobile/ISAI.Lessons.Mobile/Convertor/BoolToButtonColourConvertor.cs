using System;
using System.Globalization;
using Xamarin.Forms;

namespace ISAI.Lessons.Mobile.Convertor
{
    public class BoolToButtonColourConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!(bool)value)
            {
                return Color.FromHex("#18b8ff");
            } else
            {
                return Color.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
            //throw new NotImplementedException();
        }
    }
}
