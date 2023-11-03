using System.Globalization ;
using System.Windows ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class EnumToBoolConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         string str = parameter as string;
         if (str == null || !Enum.IsDefined(value.GetType(), value))
            return DependencyProperty.UnsetValue;
         return Enum.Parse(value.GetType(), str).Equals(value);
      }

      public object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
      {
         string str = parameter as string;
         if (str == null || value.Equals(false))
            return Binding.DoNothing;
         return Enum.Parse(targetType, str);
      }
   }
}