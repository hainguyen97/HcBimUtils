using System.Windows ;
using System.Windows.Interop ;

namespace HcBimUtils
{
   public static class ShowWindowUtils
   {
      public static IntPtr MainWindowHandle { get; set; }

      public static bool ShowDialogHandle(this Window window)
      {
         new WindowInteropHelper(window).Owner = MainWindowHandle;
         return window.ShowDialog() == true;
      }

      public static bool ShowDialogHandle(this Window window, Window parent)
      {
         new WindowInteropHelper(window).Owner = new WindowInteropHelper(parent).Handle;
         return window.ShowDialog() == true;
      }

      public static void ShowHandle(this Window window)
      {
         new WindowInteropHelper(window).Owner = MainWindowHandle;
         window.Show();
      }
      public static void ShowHandle(this Window window, Window parent)
      {
         new WindowInteropHelper(window).Owner = new WindowInteropHelper(parent).Handle;
         window.Show();
      }
   }
}