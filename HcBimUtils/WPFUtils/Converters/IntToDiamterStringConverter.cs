using System.Globalization ;
using System.Text.RegularExpressions ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class IntToDiameterStringConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         try
         {
            if (value != null)
            {
               return "Փ" + System.Convert.ToInt32(value);
            }
            return "";
         }
         catch (Exception)
         {

            return "Փ 0";
         }

      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         //remove symbol
         try
         {
            var resultString = Regex.Match(value.ToString(), @"\d+").Value;
            int.TryParse(resultString, out var n);
            return n;

         }
         catch (Exception)
         {

            return 0;
         }

      }
   }
}