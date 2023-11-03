using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils.SelectionFilter
{
    // Cách Dùng Với Func
    // var ref = ActiveData.Selection.PickObjects(ObjectType.Element,new FilterCategoryUtils{ FuncElement = x => x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFraming});
    public class FilterCategoryUtils : ISelectionFilter
    {
        public Func<Element, bool> FuncElement { get; set; }

        public Func<Reference, bool> FuncReference { get; set; }

        public bool AllowElement(Element elem)
        {
            return FuncElement(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            // return FuncReference(reference);
            return false;
        }
    }

    public class ColumnFloorSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.ToBuiltinCategory() == BuiltInCategory.OST_StructuralColumns || element is Floor)
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

    public class FloorSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.ToBuiltinCategory() == BuiltInCategory.OST_Floors)
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

    public class FoundationSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.ToBuiltinCategory() == BuiltInCategory.OST_StructuralFoundation)
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