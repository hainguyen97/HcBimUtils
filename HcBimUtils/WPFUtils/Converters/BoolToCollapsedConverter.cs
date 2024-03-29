﻿using System.Globalization ;
using System.Windows ;
using System.Windows.Data ;

namespace HcBimUtils.WPFUtils.Converters
{
   public class BoolToCollapsedConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is true)
         {
            return Visibility.Visible;
         }

         return Visibility.Collapsed;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }
}