using System.Globalization ;
using System.Windows ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class IntToVisibilityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         int integer = (int)value;
         if (integer == int.Parse(parameter.ToString()))
            return Visibility.Visible;
         return Visibility.Hidden;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }
   }
}