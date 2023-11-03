using Autodesk.Revit.DB;

namespace HcBimUtils
{
    public static class ElementIdUtils
    {
        public static ICollection<ElementId> Delete(this ElementId id, Document doc)
        {
            ICollection<ElementId> deleteIds = new List<ElementId>();
            if (id.IsValid() && doc != null)
            {
                try
                {
                    deleteIds = doc.Delete(id);
                }
                catch
                {
                    // ignored
                }
            }
            return deleteIds;
        }
    }
}
