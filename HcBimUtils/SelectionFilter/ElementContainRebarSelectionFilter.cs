using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI.Selection;

namespace HcBimUtils.SelectionFilter
{
    public class ElementContainRebarSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (IsContainRebar(element))
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }

        private bool IsContainRebar(Element ele)
        {
            var flag = false;
            var data = RebarHostData.GetRebarHostData(ele);
            if (data.GetRebarsInHost().Count > 0)
            {
                flag = true;
            }
            else if (data.GetAreaReinforcementsInHost().Count > 0)
            {
                flag = true;
            }
            else if (data.GetRebarContainersInHost().Count > 0)
            {
                flag = true;
            }

            return flag;
        }
    }
}