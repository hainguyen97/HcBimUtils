using System.Globalization ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterUnitMmFeet : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         double feet = (double)value;

         int decimalPlace = 3;
         if (parameter != null)
         {
            decimalPlace = System.Convert.ToInt32(parameter);
         }
         double mm = feet.FootToMm().RoundByDecimalPlace(decimalPlace);

         return mm;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value == null)
         {
            return 0;
         }

         if (string.IsNullOrEmpty((string)value))
         {
            return 0;
         }

         double mm = System.Convert.ToDouble(value);

         double feet = mm.MmToFoot();

         return feet;
      }
   }
}