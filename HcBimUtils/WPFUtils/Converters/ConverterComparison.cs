using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterComparison : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value?.Equals(parameter);
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value?.Equals(true) == true ? parameter : Binding.DoNothing;
      }
   }
}