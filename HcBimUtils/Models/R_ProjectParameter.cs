using Autodesk.Revit.DB ;

namespace HcBimUtils.Models
{
   public class R_ProjectParameter
   {
      public ElementId Id { get; set; }
      public Definition Definition { get; set; }
      public ElementBinding Binding { get; set; }
      public string Name { get; set; }
      public bool IsSharedStatusKnown { get; set; }
      public bool IsShared { get; set; }
      public bool IsInstance { get; set; }
      public string GUID { get; set; }
      public List<Category> GetCategories()
      {
         if (Binding != null)
         {
            return Binding.Categories.Cast<Category>().OrderBy(x => x.Name).ToList();
         }
         return null;
      }
   }
}
