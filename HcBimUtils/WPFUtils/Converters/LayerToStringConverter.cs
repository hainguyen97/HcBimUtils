using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class LayerToStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value != null)
         {
            if ((int)value == 1)
            {
               return "TOP 1";
            }
            if ((int)value == 2)
            {
               return "TOP 2";
            }
            if ((int)value == 3)
            {
               return "TOP 3";
            }
            if ((int)value == 4)
            {
               return "BOT 1";
            }
            if ((int)value == 5)
            {
               return "BOT 2";
            }
            if ((int)value == 6)
            {
               return "BOT 3";
            }
         }
         return "";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return System.Convert.ToInt32(value);
      }
   }
}