using Autodesk.Revit.DB;

namespace HcBimUtils.ViewUtils
{
    public static class ViewExtensions
    {
        public static bool IsViewOnSheet(this View view)
        {
            if (view != null)
            {
                Document doc = view.Document;
                Parameter para = view.get_Parameter(BuiltInParameter.VIEWPORT_SHEET_NAME);
                if (para != null)
                {
                    string sheetNumber = para.AsString();
                    if (!string.IsNullOrEmpty(sheetNumber))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Plane GetPlaneFromSketchPlane(this View view)
        {
            SketchPlane sketchPlane = view.SketchPlane;
            Plane plane;
            if (sketchPlane != null)
            {
                plane = sketchPlane.GetPlane();
            }
            else
            {
                plane = Plane.CreateByOriginAndBasis(view.Origin, view.RightDirection, view.UpDirection);
                sketchPlane = SketchPlane.Create(view.Document, plane);
            }
            return plane;
        }

        public static Plane GetPlaneByView(this View view)
        {
            return Plane.CreateByOriginAndBasis(view.Origin, view.RightDirection, view.UpDirection);
        }

        public static bool GetVisibility(this View view, Category category)
        {
            return category != null && category.get_AllowsVisibilityControl(view) && !view.GetCategoryHidden(category.Id);
        }

        public static void SetVisibility(this View view, Category category, bool visible)
        {
            if (category == null)
            {
                return;
            }
            if (category.get_AllowsVisibilityControl(view))
            {
                view.SetCategoryHidden(category.Id, !visible);
            }
        }
    }
}