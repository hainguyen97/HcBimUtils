using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;

namespace HcBimUtils.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Command : ExternalCommand
    {
        public override void Execute()
        {
            TaskDialog.Show(Document.Title, "HcBimUtils");
        }
    }
}