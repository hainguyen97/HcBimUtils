using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils.SelectionFilter
{
    public class WallAndSlabSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is Wall || elem is Floor)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        protected readonly HashSet<ElementId> mCategoryIds;
    }

    public class WallAndColumnSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is Wall || elem.Category.ToBuiltinCategory() == BuiltInCategory.OST_StructuralColumns)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        protected readonly HashSet<ElementId> mCategoryIds;
    }

    public class WallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is Wall)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        protected readonly HashSet<ElementId> mCategoryIds;
    }

    public class ColumnSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.ToBuiltinCategory() == BuiltInCategory.OST_StructuralColumns)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        protected readonly HashSet<ElementId> mCategoryIds;
    }

    public class JtElementsOfClassSelectionFilter<T> : ISelectionFilter where T : Element
    {
        public bool AllowElement(Element e)
        {
            return e is T;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }

    public class ElementIdFilter : ISelectionFilter
    {
        private ElementId _id;

        public ElementIdFilter(ElementId id)
        {
            _id = id;
        }

        public bool AllowElement(Element e)
        {
            return e.Id == _id;
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }

    public class PointOnElementFilter : ISelectionFilter
    {
        private ElementId _id;

        public PointOnElementFilter(ElementId id)
        {
            _id = id;
        }

        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            if (reference.ElementId == _id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class ElementIdsFilter : ISelectionFilter
    {
        private List<ElementId> _ids;

        public ElementIdsFilter(List<ElementId> ids)
        {
            _ids = ids;
        }

        public bool AllowElement(Element e)
        {
            return _ids.Contains(e.Id);
        }

        public bool AllowReference(Reference r, XYZ p)
        {
            return true;
        }
    }
}