using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HcBimUtils.SelectionFilter
{
    public class GridSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category != null)
            {
                if (element.Category.Name == "Grids")
                {
                    return true;
                }
            }

            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }

    public class LevelSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is Level)
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