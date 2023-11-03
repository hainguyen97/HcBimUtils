using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils.Geometry
{
    public class GGeomTools
    {
        public static XYZ FindMiddlePoint(XYZ point1, XYZ point2)
        {
            return point1.Add(point2.Subtract(point1).Multiply(0.5));
        }

        public static XYZ Multiply(XYZ vector, double ratio)
        {
            var num = vector.X * ratio;
            var num2 = vector.Y * ratio;
            var num3 = vector.Z * ratio;
            return new XYZ(num, num2, num3);
        }

        public static List<XYZ> PutOnToY(List<XYZ> poly, XYZ vecShapeNormal)
        {
            if (IsColinear(vecShapeNormal, XYZ.BasisZ))
            {
                return poly;
            }
            var num = vecShapeNormal.AngleOnPlaneTo(XYZ.BasisZ, vecShapeNormal.CrossProduct(XYZ.BasisZ));
            var transform = Transform.CreateRotation(vecShapeNormal.CrossProduct(XYZ.BasisZ), num);
            return (from xyz in poly select transform.OfPoint(xyz)).ToList();
        }

        public static List<UV> GetUv(IEnumerable<XYZ> poly)
        {
            return (from xyz in poly select new UV(xyz.X, xyz.Y)).ToList();
        }

        public static double SignedAreaPoly2D(List<UV> poly)
        {
            var count = poly.Count;
            var num = 0.0;
            for (var i = 0; i < count; i++)
            {
                num += (poly[i].V + poly[(i + 1) % count].V) * (poly[(i + 1) % count].U - poly[i].U);
            }
            return num / 2.0;
        }

        public static bool IsClockWise(List<UV> poly)
        {
            return SignedAreaPoly2D(poly) > 0.0;
        }

        public static bool IsColinear(XYZ vecFirst, XYZ vecSecond)
        {
            var xyz = vecFirst.CrossProduct(vecSecond);
            return xyz.X.IsEqual(0.0) && xyz.Y.IsEqual(0.0) && xyz.Z.IsEqual(0.0);
        }

        public static bool AreCodirectional(XYZ vecA, XYZ vecB)
        {
            var xyz = vecA.Normalize();
            var xyz2 = vecB.Normalize();
            return xyz.IsAlmostEqualTo(xyz2);
        }

        public static bool AreCodirectional(XYZ vecA, XYZ vecB, double tolerance)
        {
            return vecA.Normalize().IsAlmostEqualTo(vecB.Normalize(), tolerance);
        }

        public static List<Curve> ComputeCurves(List<XYZ> poly, List<double> bulges, bool withArcs, double shortCurveTolerance)
        {
            var list = new List<Curve>();
            var count = poly.Count;
            for (var i = 0; i < count - 1; i++)
            {
                var xyz = poly[i];
                var xyz2 = poly[i + 1];
                var previousPoint = i > 0 ? poly[i - 1] : null;
                var nextPoint = i < count - 2 ? poly[i + 2] : null;
                var bulgeAsDistance = bulges != null && bulges.Count > i ? bulges[i] : 0.0;
                if (!(xyz.DistanceTo(xyz2) > shortCurveTolerance)) continue;
                var curve = ComputeCurve(xyz, xyz2, bulgeAsDistance, previousPoint, nextPoint, withArcs);
                if (curve != null)
                {
                    list.Add(curve);
                }
            }
            return list;
        }

        private static Curve ComputeCurve(XYZ startPoint, XYZ endPoint, double bulgeAsDistance, XYZ previousPoint, XYZ nextPoint, bool withArcs)
        {
            if (startPoint == null || endPoint == null)
            {
                return null;
            }
            var xyz = (endPoint - startPoint).Normalize();
            XYZ xyz2 = null;
            if (previousPoint != null)
            {
                xyz2 = (startPoint - previousPoint).Normalize().CrossProduct(xyz);
            }
            else if (nextPoint != null)
            {
                xyz2 = xyz.CrossProduct((nextPoint - endPoint).Normalize());
            }
            if (!withArcs || bulgeAsDistance.IsEqual(0.0, 1E-06) || xyz2 == null || xyz2.IsZeroLength())
            {
                return Line.CreateBound(startPoint, endPoint);
            }
            var xyz3 = xyz.CrossProduct(xyz2).Normalize();
            if (xyz3.IsAlmostEqualTo(XYZ.Zero, 1E-06))
            {
                return Line.CreateBound(startPoint, endPoint);
            }
            var xyz4 = startPoint / 2.0 + endPoint / 2.0 + xyz3 * Math.Abs(bulgeAsDistance);
            return Arc.Create(startPoint, endPoint, xyz4);
        }

        public static List<Curve> OffsetCurves(List<Curve> curves, XYZ vecOffset)
        {
            if (curves == null)
            {
                return null;
            }

            var transform = Transform.CreateTranslation(vecOffset);
            return (from curve in curves select curve.CreateTransformed(transform)).ToList();
        }

        public static XYZ GetPerpendicularVector(XYZ vector)
        {
            var xyz = new XYZ(vector.X, -vector.Y, vector.Z);
            var xyz2 = vector.CrossProduct(xyz).Normalize();
            if (!xyz2.IsZeroLength() && xyz2.DotProduct(vector).IsEqual(0.0))
            {
                return xyz2;
            }
            xyz = new XYZ(vector.X, -vector.Z, vector.Y);
            xyz2 = vector.CrossProduct(xyz).Normalize();
            if (!xyz2.IsZeroLength() && xyz2.DotProduct(vector).IsEqual(0.0))
            {
                return xyz2;
            }
            xyz = new XYZ(vector.Z, vector.Y, vector.X);
            xyz2 = vector.CrossProduct(xyz).Normalize();
            if (!xyz2.IsZeroLength() && xyz2.DotProduct(vector).IsEqual(0.0))
            {
                return xyz2;
            }
            return new XYZ(1.0, 1.0, 1.0);
        }

        public static bool ParallelVectors(XYZ vectorA, XYZ vectorB)
        {
            return Math.Abs(vectorA.Normalize().DotProduct(vectorB.Normalize())).IsEqual(1.0);
        }
    }
}