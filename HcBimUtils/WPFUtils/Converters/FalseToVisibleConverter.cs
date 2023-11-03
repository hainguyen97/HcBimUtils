using System.Globalization ;
using System.Windows ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class FalseToVisibleConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is false)
         {
            return Visibility.Visible;
         }

         return Visibility.Hidden;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}