namespace HcBimUtils.WarmingUtils.View
{
   public static class ShowError2
   {
      public static void Show(string error)
      {
         var view = new ErrroView(error);
         view.ShowDialog();
      }
   }
}