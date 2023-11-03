using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HcBimUtils.SelectionFilter
{
    public class TextNoteSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.Name == "Text Notes")
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

    // Sử Dụng Function để lọc đối tượng

    public class FuncSelectionFilter : ISelectionFilter
    {
        public Func<Element, bool> FuncElement;
        public Func<Reference, bool> FuncReference { get; set; }

        public FuncSelectionFilter(BuiltInCategory Category)
        {
            FuncElement = x => x.Category.Id.IntegerValue == (int)Category;
        }

        public FuncSelectionFilter(BuiltInCategory Category, BuiltInCategory Category1)
        {
            FuncElement = x => (x.Category.Id.IntegerValue == (int)Category || x.Category.Id.IntegerValue == (int)Category1);
        }

        public bool AllowElement(Element elem)
        {
            return FuncElement(elem);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return FuncReference(reference);
        }
    }
}