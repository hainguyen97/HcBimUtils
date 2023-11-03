using System.Collections;
using Autodesk.Revit.DB;
using HcBimUtils.GeometryUtils;

namespace HcBimUtils
{
    public static class CurveUtils
    {
        public static XYZ Direction(this Curve curve)
        {
            return (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();
        }

        public static XYZ Midpoint(this Curve curve)
        {
            double endParameter = curve.GetEndParameter(0);
            double num = (curve.GetEndParameter(1) - endParameter) * 0.5;
            num += endParameter;
            return curve.Evaluate(num, false);
        }

        public static Curve FindCurveByDirection(EdgeArray edgeArray, XYZ direction)
        {
            Curve curveByDirection = null;
            foreach (Edge edge in edgeArray)
            {
                Curve curve = edge.AsCurve();

                if (curve.Direction().IsCodirectionalTo(direction))
                {
                    curveByDirection = curve;
                    break;
                }
            }

            return curveByDirection;
        }

        public static List<Curve> GetCurvesParallelWithDirection(List<Curve> curves, XYZ direction)
        {
            List<Curve> parallelCurves = new List<Curve>();
            foreach (Line line in curves)
            {
                if (line.Direction.IsCodirectionalTo(direction))
                {
                    parallelCurves.Add(line);
                }
            }
            return parallelCurves;
        }

        // Lấy curve song song với direction
        public static Curve GetCurveParallelWithDirection(List<Curve> curves, XYZ direction)
        {
            Curve topCurve = null;
            foreach (Line line in curves)
            {
                if (line.Direction.IsCodirectionalTo(direction))
                {
                    topCurve = line;
                    break;
                }
            }
            return topCurve;
        }

        public static List<XYZ> GetIntersections(Curve c1, Curve c2, bool isUnbound)
        {
            List<XYZ> valueList = new List<XYZ>();
            IntersectionResultArray array;
            if (c1 != null && c2 != null)
            {
                if (isUnbound)
                {
                    Curve cu1 = c1.Clone();
                    Curve cu2 = c2.Clone();
                    cu1.MakeUnbound();
                    cu2.MakeUnbound();
                    SetComparisonResult result = cu1.Intersect(cu2, out array);
                    if (array != null)
                    {
                        foreach (IntersectionResult item in array)
                        {
                            valueList.Add(item.XYZPoint);
                        }
                    }
                }
                else
                {
                    SetComparisonResult result = c1.Intersect(c2, out array);
                    if (array != null)
                    {
                        foreach (IntersectionResult item in array)
                        {
                            valueList.Add(item.XYZPoint);
                        }
                    }
                }
            }
            return valueList;
        }

        public static Curve BreakCurve(Curve curve, XYZ point)
        {
            Curve newCurve = curve.Clone();
            double distance = curve.Project(point).Distance;
            if (distance < Constants.Eps)
            {
                double paraIntersection = curve.Project(point).Parameter;
                curve.MakeBound(paraIntersection, curve.GetEndParameter(1));
                newCurve.MakeBound(newCurve.GetEndParameter(0), paraIntersection);
            }
            return newCurve;
        }

        public static Curve ExpandToContain(this Curve curve, XYZ point)
        {
            //nếu point nằm ngoài đường curve kéo dài thì return curve
            Curve unboundCurve = curve.Clone();
            unboundCurve.MakeUnbound();
            if (!unboundCurve.IsContain(point))
            {
                return curve;
            }

            //nếu curve nằm trên đường curve kéo dài nhưng không nằm giữa 2 điểm thì return curve
            if (curve.IsContain(point))
            {
                return curve;
            }

            if (curve is Line)
            {
                Line line = curve as Line;
                XYZ startPoint = line.GetEndPoint(0);
                XYZ endPoint = line.GetEndPoint(1);
                if (point.DistanceTo(startPoint) > point.DistanceTo(endPoint))
                {
                    return Line.CreateBound(startPoint, point);
                }
                else
                {
                    return Line.CreateBound(point, endPoint);
                }
            }
            else if (curve is Arc)
            {
                Arc arc = curve as Arc;
                XYZ center = arc.Center;
                double radius = arc.Radius;

                XYZ startPoint = arc.GetEndPoint(0);
                XYZ endPoint = arc.GetEndPoint(1);

                if (point.DistanceTo(startPoint) > point.DistanceTo(endPoint))
                {
                    return Arc.Create(startPoint, point, endPoint);
                }
                else
                {
                    return Arc.Create(point, endPoint, startPoint);
                }
            }
            return curve;
        }

        // Kiểm tra điểm có thuộc curve không
        public static bool IsContain(this Curve curve, XYZ point)
        {
            bool isContain = false;
            double distance = curve.Project(point).Distance;
            if (distance < Constants.Eps)
            {
                isContain = true;
            }
            return isContain;
        }

        public static bool Contains(this Curve Cu, XYZ p)
        {
            XYZ a = Cu.GetEndPoint(0);
            XYZ b = Cu.GetEndPoint(1);
            double f = Cu.ApproximateLength;
            double da = a.DistanceTo(p);
            double db = p.DistanceTo(b);
            // da + db is always greater or equal f
            return ((da + db) - f) * f < f * 0.0001;
        }

        public static XYZ SP(this Curve curve)
        {
            try
            {
                return curve.Tessellate()[0];
            }
            catch
            {
                return curve.GetEndPoint(0);
            }
        }

        public static XYZ EP(this Curve curve)
        {
            try
            {
                IList<XYZ> xyzList = curve.Tessellate();
                return xyzList[xyzList.Count - 1];
            }
            catch
            {
                return curve.GetEndPoint(1);
            }
        }

        public static Line ProjectOntoPlane(this Curve curve, BPlane plane)
        {
            var sp = curve.SP().ProjectOnto(plane);
            var ep = curve.EP().ProjectOnto(plane);
            return Line.CreateBound(sp, ep);
        }

        public static XYZ GetIntersectPoint(this Curve curve1, Curve curve2)
        {
            XYZ endPoint1 = curve1.GetEndPoint(0);
            XYZ endPoint2 = curve1.GetEndPoint(1);
            XYZ endPoint3 = curve2.GetEndPoint(0);
            XYZ endPoint4 = curve2.GetEndPoint(1);
            XYZ source1 = endPoint2 - endPoint1;
            XYZ source2 = endPoint3 - endPoint1;
            double num1 = source1.DotProduct(source2) / (source1.GetLength() * source2.GetLength());
            XYZ xyz1 = endPoint1 + source2.GetLength() * num1 * source1.Normalize() - endPoint3;
            double length = xyz1.GetLength();
            XYZ source3 = endPoint4 - endPoint3;
            double num3 = xyz1.DotProduct(source3) / (xyz1.GetLength() * source3.GetLength());
            XYZ xyz2 = endPoint3 + length / num3 * source3.Normalize();
            XYZ xyz3 = xyz2 - endPoint1;
            (xyz3.CrossProduct(source1) / (xyz3.GetLength() * source1.GetLength())).GetLength();
            return xyz2;
        }

        public static bool IsEqual(this Curve curve1, Curve curve2)
        {
            var u1 = curve1.Direction();
            var u2 = curve2.Direction();
            var l1 = curve1.Length;
            var l2 = curve2.Length;
            var mid1 = curve1.Midpoint();
            var mid2 = curve2.Midpoint();
            if (u1.IsParallel(u2) && l1 == l2 && mid1.IsAlmostEqualTo(mid2))
            {
                return true;
            }
            return false;
        }

        public static XYZ GetProjectPointOnCurve(this Curve curve, XYZ point, bool makeUnbound = true)
        {
            XYZ projectPoint = null;
            if (curve != null && point != null)
            {
                Curve cloneCurve = curve.Clone();

                if (makeUnbound)
                {
                    cloneCurve.MakeUnbound();
                }

                projectPoint = cloneCurve.Project(point).XYZPoint;
            }
            return projectPoint;
        }

        public static XYZ LineLineIntersection(XYZ x1, XYZ x2, XYZ x3, XYZ x4)
        {
            XYZ xyz = x2 - x1;
            XYZ xyz2 = x4 - x3;
            XYZ xyz3 = x3 - x1;
            if (Math.Round(Math.Abs(xyz3.DotProduct(xyz.CrossProduct(xyz2))), 2) > 0.03)
            {
                return null;
            }
            double num = xyz3.CrossProduct(xyz2).DotProduct(xyz.CrossProduct(xyz2));
            double num2 = Math.Pow(xyz.CrossProduct(xyz2).GetLength(), 2.0);
            if (Math.Abs(num2) < 0.001)
            {
                return null;
            }
            double num3 = num / num2;
            return x1 + xyz * num3;
        }

        public static void GetConnectedCurves(this Curve thisCurve, List<Curve> curves, bool checkWithEnd1, bool checkWithEnd2, out List<Curve> connectedCurvesWithEnd1, out List<Curve> connectedCurvesWithEnd2)
        {
            if (thisCurve == null)
            {
                throw new ArgumentNullException("thisCurve");
            }
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }
            connectedCurvesWithEnd1 = new List<Curve>();
            connectedCurvesWithEnd2 = new List<Curve>();
            if (curves.Count < 1)
            {
                return;
            }
            XYZ endPoint = thisCurve.GetEndPoint(0);
            XYZ endPoint2 = thisCurve.GetEndPoint(1);
            double num = 1E-06;
            foreach (Curve curve in curves)
            {
                if (thisCurve.Intersect(curve) != SetComparisonResult.Equal)
                {
                    XYZ endPoint3 = curve.GetEndPoint(0);
                    XYZ endPoint4 = curve.GetEndPoint(1);
                    if (checkWithEnd2 && (endPoint2.DistanceTo(endPoint3) < num || endPoint2.DistanceTo(endPoint4) < num))
                    {
                        connectedCurvesWithEnd2.Add(curve);
                    }
                    else if (checkWithEnd1 && (endPoint.DistanceTo(endPoint3) < num || endPoint.DistanceTo(endPoint4) < num))
                    {
                        connectedCurvesWithEnd1.Add(curve);
                    }
                }
            }
        }

        public static List<Curve> GetConnectedCurves(this Curve thisCurve, List<Curve> curves)
        {
            if (thisCurve == null)
            {
                throw new ArgumentNullException("thisCurve");
            }
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }

            thisCurve.GetConnectedCurves(curves, true, true, out var distinctCurves, out var collection);
            distinctCurves.AddRange(collection);
            distinctCurves = distinctCurves.GetDistinctCurves();
            return distinctCurves;
        }

        public static List<Curve> GetDistinctCurves(this List<Curve> curves)
        {
            List<Curve> list = new List<Curve>(curves);
            for (int i = 0; i < list.Count; i++)
            {
                Curve otherCurve = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].IsSame(otherCurve))
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
            return list;
        }

        public static bool IsSame(this Curve curve, Curve otherCurve)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }
            if (otherCurve == null)
            {
                throw new ArgumentNullException("otherCurve");
            }
            double num = 1E-05;
            bool result = false;
            if (curve.Intersect(otherCurve, out _) == SetComparisonResult.Equal)
            {
                XYZ endPoint = curve.GetEndPoint(0);
                XYZ endPoint2 = curve.GetEndPoint(1);
                XYZ endPoint3 = otherCurve.GetEndPoint(0);
                XYZ endPoint4 = otherCurve.GetEndPoint(1);
                if ((endPoint.DistanceTo(endPoint3) < num && endPoint2.DistanceTo(endPoint4) < num) || (endPoint.DistanceTo(endPoint4) < num && endPoint2.DistanceTo(endPoint3) < num))
                {
                    result = true;
                }
            }
            return result;
        }

        public static Curve ProjectOn(this Curve curve, BPlane plane)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }
            if (plane == null)
            {
                throw new ArgumentNullException("plane");
            }
            XYZ normal = plane.Normal;
            XYZ endPoint = curve.GetEndPoint(0);
            Transform transform = Transform.CreateTranslation(normal.DotProduct(plane.Origin - endPoint) * normal);
            return curve.CreateTransformed(transform);
        }

        public static Curve GetCurveHasPoint(this IList<Curve> curves, XYZ point, out int curveIndex)
        {
            curveIndex = -1;
            Curve result = null;
            foreach (Curve curve in curves)
            {
                curveIndex++;
                if (point.IsOnCurve(curve))
                {
                    result = curve;
                    break;
                }
            }
            return result;
        }

        public static XYZ GetIntersectionPoint(this Curve thisCurve, Curve otherCurve, bool makeUnbound = false)
        {
            if (thisCurve == null)
            {
                throw new ArgumentNullException("thisCurve");
            }
            if (otherCurve == null)
            {
                throw new ArgumentNullException("otherCurve");
            }
            XYZ result = null;
            Curve thisCurveClone = thisCurve.Clone();
            Curve otherCurveClone = otherCurve.Clone();

            if (makeUnbound)
            {
                thisCurveClone.MakeUnbound();
                otherCurveClone.MakeUnbound();
            }

            IntersectionResultArray intersectionResultArray;
            if (thisCurveClone.Intersect(otherCurveClone, out intersectionResultArray) == SetComparisonResult.Overlap)
            {
                IEnumerator enumerator = intersectionResultArray.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    result = ((IntersectionResult)enumerator.Current)?.XYZPoint;
                }
            }
            return result;
        }

        public static XYZ GetIntersectionPointByExtend(this Curve thisCurve, Curve otherCurve, double extend = 100)
        {
            if (thisCurve == null)
            {
                throw new ArgumentNullException("thisCurve");
            }
            if (otherCurve == null)
            {
                throw new ArgumentNullException("otherCurve");
            }

            var c1 = Line.CreateBound(thisCurve.SP(), thisCurve.EP()).ExtendLineBothEnd(extend);
            var c2 = Line.CreateBound(otherCurve.SP(), otherCurve.EP()).ExtendLineBothEnd(extend);
            XYZ result = null;
            IntersectionResultArray intersectionResultArray;
            if (c1.Intersect(c2, out intersectionResultArray) == SetComparisonResult.Overlap)
            {
                IEnumerator enumerator = intersectionResultArray.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    result = ((IntersectionResult)enumerator.Current)?.XYZPoint;
                }
            }
            return result;
        }

        public static List<XYZ> GetIntersectionPoints(this Curve thisCurve, List<Curve> curves)
        {
            if (thisCurve == null)
            {
                throw new ArgumentNullException("thisCurve");
            }
            if (curves == null)
            {
                throw new ArgumentNullException("curves");
            }
            List<XYZ> list = new List<XYZ>();
            foreach (Curve curve in curves)
            {
                IntersectionResultArray intersectionResultArray;
                SetComparisonResult setComparisonResult = thisCurve.Intersect(curve, out intersectionResultArray);
                if (setComparisonResult == SetComparisonResult.Overlap || setComparisonResult == SetComparisonResult.Equal)
                {
                    if (intersectionResultArray == null)
                    {
                        XYZ endPoint = curve.GetEndPoint(0);
                        if (!endPoint.Iscontains(list))
                        {
                            list.Add(endPoint);
                        }
                    }
                    else
                    {
                        foreach (object obj in intersectionResultArray)
                        {
                            IntersectionResult intersectionResult = (IntersectionResult)obj;
                            if (!intersectionResult.XYZPoint.Iscontains(list))
                            {
                                list.Add(intersectionResult.XYZPoint);
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static XYZ ComputeCentroid(this CurveLoop curves)
        {
            double num = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = 0.0;
            XYZ xyz = new XYZ();
            XYZ xyz2 = new XYZ();
            int num5 = 0;
            foreach (Curve curve in curves)
            {
                Arc arc = curve as Arc;
                XYZ endPoint = curve.GetEndPoint(1);
                if (num5 == 0)
                {
                    xyz = curve.GetEndPoint(0);
                    xyz2 = endPoint;
                }
                else
                {
                    XYZ xyz3 = endPoint - xyz;
                    XYZ xyz4 = endPoint - xyz2;
                    double num7;
                    if (arc != null)
                    {
                        double num6 = xyz3.AngleTo(xyz4);
                        num7 = arc.Radius * arc.Radius * num6 / 3.1415926535897931;
                    }
                    else
                    {
                        num7 = xyz3.CrossProduct(xyz4).GetLength() / 2.0;
                    }
                    num += num7 * (xyz.X + xyz2.X + endPoint.X) / 3.0;
                    num2 += num7 * (xyz.Y + xyz2.Y + endPoint.Y) / 3.0;
                    num3 += num7 * (xyz.Z + xyz2.Z + endPoint.Z) / 3.0;
                    num4 += num7;
                    xyz2 = endPoint;
                }
                num5++;
            }
            if (num4 <= 0.0)
            {
                return (xyz2 - xyz) / 2.0;
            }
            return new XYZ(num / num4, num2 / num4, num3 / num4);
        }

        public static bool IsIntersect(this Curve c1, Curve c2)
        {
            var b = c1.Intersect(c2);

            if (b == SetComparisonResult.Overlap)
            {
                return true;
            }
            return false;
        }
    }
}