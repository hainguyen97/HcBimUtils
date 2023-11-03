using System.Diagnostics ;
using System.Windows ;

namespace HcBimUtils.WarmingUtils.View
{
   /// <summary>
   /// Interaction logic for ErrroView.xaml
   /// </summary>
   public partial class ErrroView : Window
   {
      public string TextError { set; get; } = "";

      public ErrroView(string error)
      {
         TextError = error;
         DataContext = this;
         InitializeComponent();
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         Process.Start("https://www.facebook.com/");
      }
   }
}