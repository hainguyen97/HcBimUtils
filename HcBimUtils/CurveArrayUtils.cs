using Autodesk.Revit.DB;

namespace HcBimUtils
{
    public static class CurveArrayUtils
    {
        public static List<Curve> ToCurves(this CurveArray curveArray)
        {
            List<Curve> curves = new List<Curve>();
            foreach (Curve curve in curveArray)
            {
                curves.Add(curve);
            }
            return curves;
        }
    }
}
