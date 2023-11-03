using Autodesk.Revit.DB;

namespace HcBimUtils.Comparer
{
    public class XYZComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            if (second != null && first != null && first.Z.IsEqual(second.Z))
            {
                if (!first.Y.IsEqual(second.Y))
                {
                    if (!first.Y.IsGreater(second.Y))
                    {
                        return -1;
                    }
                    return 1;
                }

                if (first.X.IsEqual(second.X))
                {
                    return 0;
                }
                if (first.X.IsGreater(second.X))
                {
                    return 1;
                }
                return -1;
            }

            if (second != null && first != null && !first.Z.IsGreater(second.Z))
            {
                return -1;
            }
            return 1;
        }
    }
}