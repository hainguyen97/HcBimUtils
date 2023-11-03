using Autodesk.Revit.DB;

namespace HcBimUtils
{
    public static class CurveLoopUtils
    {
        public static List<XYZ> GetPoints(this CurveLoop curves)
        {
            List<XYZ> points = new List<XYZ>();
            foreach (Curve curve in curves)
            {
                points.Add(curve.SP());
            }

            return points;
        }
        public static CurveLoop CreateCurveLoop(List<XYZ> pts)
        {
            int n = pts.Count;
            CurveLoop curveLoop = new CurveLoop();
            for (int i = 1; i < n; ++i)
            {
                curveLoop.Append(Line.CreateBound(
                    pts[i - 1], pts[i]));
            }
            curveLoop.Append(Line.CreateBound(
                pts[n - 1], pts[0]));
            return curveLoop;
        }
        public static CurveArray ToCurveArray(this CurveLoop curveLoop)
        {
            CurveArray curveArray = new CurveArray();
            foreach (Curve curve in curveLoop)
            {
                curveArray.Append(curve);
            }
            return curveArray;
        }
    }
}
