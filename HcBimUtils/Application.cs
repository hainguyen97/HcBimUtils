using HcBimUtils.Commands;
using Nice3point.Revit.Toolkit.External;

namespace HcBimUtils
{
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CreateRibbon();
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Commands", "HcBimUtils");

            var showButton = panel.AddPushButton<Command>("Execute");
            showButton.SetImage("/HcBimUtils;component/Resources/Icons/RibbonIcon16.png");
            showButton.SetLargeImage("/HcBimUtils;component/Resources/Icons/RibbonIcon32.png");
        }
    }
}