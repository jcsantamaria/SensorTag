using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace SensorTagPi.Core
{
    class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            bool inverted = parameter != null && parameter.ToString().ToLower().CompareTo("inverted") == 0;
            var trueValue  = inverted ? Visibility.Collapsed : Visibility.Visible;
            var falseValue = inverted ? Visibility.Visible : Visibility.Collapsed;
            if (value is bool && (bool)value)
            {
                return trueValue;
            }
            return falseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            bool inverted = parameter != null && parameter.ToString().ToLower().CompareTo("inverted") == 0;
            var trueValue  = inverted ? true : false;
            var falseValue = inverted ? false : true;

            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return trueValue;
            }
            return falseValue;
        }
    }
}
