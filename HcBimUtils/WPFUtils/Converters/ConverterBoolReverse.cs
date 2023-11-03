using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterBoolReverse : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return !System.Convert.ToBoolean(value);
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return !System.Convert.ToBoolean(value);
      }
   }
   public class ConverterIsNull : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value == null;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}