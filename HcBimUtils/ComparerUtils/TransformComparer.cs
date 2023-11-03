using Autodesk.Revit.DB;

namespace HcBimUtils.ComparerUtils
{
    public class TransformComparer : IEqualityComparer<Transform>
    {
        public bool Equals(Transform x, Transform y)
        {
            return y != null && x != null && (x.Origin - y.Origin).GetLength() < Constants.ShortCurveTolerance;
        }

        public int GetHashCode(Transform obj)
        {
            var length = Convert.ToInt32(obj.Origin.GetLength() * 100);
            var hashCode = length.GetHashCode();
            return hashCode;
        }
    }
}