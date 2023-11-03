using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils.SelectionFilter
{
    public class BeamSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category == null)
            {
                return false;
            }
            if (element.Category.ToBuiltinCategory() == BuiltInCategory.OST_StructuralFraming)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
}