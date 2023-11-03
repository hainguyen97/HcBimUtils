using System.Windows.Data ;
using System.Xml ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class ConverterXmlToString : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         if (value == null)
         {
            return "";
         }
         XmlNode node = value as XmlNode;
         if (node != null)
         {
            return node.InnerText;
         }
         else
         {
            return value.ToString();
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         return "";
      }
   }
}