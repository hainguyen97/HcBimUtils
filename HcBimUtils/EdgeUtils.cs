using Autodesk.Revit.DB;

namespace HcBimUtils
{
    public static class EdgeUtils
    {
        public static XYZ SP(this Edge edge)
        {
            try
            {
                return edge.Tessellate()[0];
            }
            catch
            {
                return edge.Evaluate(0);
            }
        }

        public static XYZ EP(this Edge edge)
        {
            try
            {
                IList<XYZ> xyzList = edge.Tessellate();
                return xyzList[xyzList.Count - 1];
            }
            catch
            {
                return edge.Evaluate(1);
            }
        }
        public static XYZ Direction(this Edge edge)
        {
            return edge.EP() - edge.SP();
        }
        public static XYZ Midpoint(this Edge edge)
        {
            var curve = edge.AsCurve();
            return CurveUtils.Midpoint(curve);
        }
    }
}
