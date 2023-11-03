using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class IntToStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value != null && System.Convert.ToInt32(value) != 0)
         {
            return System.Convert.ToInt32(value).ToString();
         }
         return "";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return System.Convert.ToInt32(value);
      }
   }
}