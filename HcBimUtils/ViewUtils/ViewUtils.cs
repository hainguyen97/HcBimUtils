using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils.ViewUtils
{
    public class ViewUtils
    {
        public static List<ViewSheet> GetAllSheets(Document doc)
        {
            return new FilteredElementCollector(doc)
               .WhereElementIsNotElementType()
               .OfClass(typeof(ViewSheet))
               .Cast<ViewSheet>()
               .OrderBy(x => x.SheetNumber)
               .ToList();
        }

        public static List<View> GetAllViews(Document doc)
        {
            return new FilteredElementCollector(doc)
               .WhereElementIsNotElementType()
               .OfClass(typeof(View))
               .Cast<View>()
               .Where(x => x.IsTemplate == false)
               .Where(x => x.GetTypeId().IntegerValue > 1)
               .Where(x => x.CanViewBeDuplicated(ViewDuplicateOption.Duplicate))
               .Where(x => x.OwnerViewId.IntegerValue == -1)
               .OrderBy(x => x.Name)
               .ToList();
        }

        public static List<T> GetSelectedElements<T>(UIDocument uidoc) where T : Element
        {
            return uidoc.Selection.GetElementIds()
               .Select(x => uidoc.Document.GetElement(x) as T)
               .Where(x => x.IsValidElement())
               .ToList();
        }

        public static string GetAssociatedLevel(View view)
        {
            string assLevel = "";
            if (view.GenLevel != null)
            {
                assLevel = view.GenLevel.Name;
            }
            return assLevel;
        }

        public static string GetViewType(View view)
        {
            return ParameterUtilities.ValueString(view.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM));
        }

        public static View3D GetA3DView()
        {
            var view3D = new FilteredElementCollector(AC.Document).OfClass(typeof(View3D)).Cast<View3D>()
                .FirstOrDefault(x => x.Name.Contains("{3D}"));
            return view3D;
        }

        public static View3D GetView3D()
        {
            var view3D = new FilteredElementCollector(AC.Document).OfClass(typeof(View3D)).Cast<View3D>()
                .FirstOrDefault(x => x.Name == "3DView_Please_Never_Modify_This_View");
            if (view3D == null)
            {
                var vft = new FilteredElementCollector(AC.Document).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);
                if (vft != null)
                {
                    view3D = View3D.CreateIsometric(AC.Document, vft.Id);
                    view3D.Name = "3DView_Please_Never_Modify_This_View";
                }
            }

            return view3D;
        }

        public static SketchPlane SetSketchPlane()
        {
            var plane = Plane.CreateByNormalAndOrigin(AC.ActiveView.ViewDirection, AC.ActiveView.Origin);
            var sk = SketchPlane.Create(AC.Document, plane);
            AC.ActiveView.SketchPlane = sk;
            return sk;
        }
    }
}