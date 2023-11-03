using System.Windows ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterEnumVisibility : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value.ToString() == parameter.ToString())
         {
            return Visibility.Visible;
         }
         else
         {
            return Visibility.Collapsed;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return value?.Equals(true) == true ? parameter : Binding.DoNothing;
      }
   }
}