using Autodesk.Revit.DB.Structure;

namespace HcBimUtils.ComparerUtils
{
    public class RebarComparer : IEqualityComparer<Rebar>
    {
        public bool Equals(Rebar x, Rebar y)
        {
#if R24
            return y != null && x != null && x.Id.Value == y.Id.Value;
#else
            return y != null && x != null && x.Id.IntegerValue == y.Id.IntegerValue;
#endif
        }

        public int GetHashCode(Rebar obj)
        {
            return 0;
        }
    }
}