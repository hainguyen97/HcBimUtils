using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class FeetToIntString : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value != null && value?.ToString() == "0")
         {
            return "";
         }

         return Math.Round(System.Convert.ToDouble(value).FootToMm()).ToString(CultureInfo.InvariantCulture);
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         int ret = 0;
         return int.TryParse((string)value, out ret) ? ret.MmToFoot() : 0;
      }
   }
}