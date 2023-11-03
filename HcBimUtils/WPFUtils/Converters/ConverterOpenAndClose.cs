using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterOpenAndClose : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool flag = (bool)value;
         return !flag;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool flag = (bool)value;
         return !flag;
      }
   }
}