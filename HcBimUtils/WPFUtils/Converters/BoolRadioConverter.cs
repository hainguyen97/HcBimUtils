using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class BoolRadioConverter : IValueConverter
   {
      public bool Inverse { get; set; }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool boolValue = (bool)value;

         return !boolValue;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool boolValue = (bool)value;

         return !boolValue;
      }
   }
}