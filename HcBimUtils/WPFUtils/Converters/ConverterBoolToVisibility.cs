using System.Globalization ;
using System.Windows.Data ;
using Visibility = System.Windows.Visibility;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterBoolToVisibility : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool isTrue = (bool)value;
         if (isTrue)
         {
            return Visibility.Visible;
         }
         else
         {
            return Visibility.Collapsed;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
   public class ConverterBoolToVisibilityReverse : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         bool isTrue = (bool)value;
         if (isTrue == false)
         {
            return Visibility.Visible;
         }
         else
         {
            return Visibility.Collapsed;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}