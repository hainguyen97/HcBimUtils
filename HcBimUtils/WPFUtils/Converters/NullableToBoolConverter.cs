using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class NullableToBoolConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is true)
         {
            return true;
         }
         return false;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return parameter;
      }
   }
}