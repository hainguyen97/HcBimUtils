using Autodesk.Revit.DB;
using HcBimUtils.Models;

namespace HcBimUtils
{
    public static class XYZUtils
    {
        public static bool DoesPointListContainCenterPoint(List<XYZ> points)
        {
            if (points.Count < 3)
            {
                return false;
            }
            for (int i = 0; i < points.Count; i++)
            {
                XYZ currentPoint = points[i];
                XYZ previousPoint = null;
                XYZ nextPoint = null;

                if (i == 0)
                {
                    previousPoint = points[points.Count - 1];
                    nextPoint = points[i + 1];
                }
                else if (i == points.Count - 1)
                {
                    previousPoint = points[i - 1];
                    nextPoint = points[0];
                }
                else
                {
                    previousPoint = points[i - 1];
                    nextPoint = points[i + 1];
                }

                XYZ direction1 = (previousPoint - currentPoint).Normalize();
                XYZ direction2 = (nextPoint - currentPoint).Normalize();

                if (direction1.CrossProduct(direction2).IsAlmostEqualTo(XYZ.Zero))
                {
                    points.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        public static bool IsExist(this XYZ point, List<XYZ> points)
        {
            if (point == null)
            {
                throw new ArgumentNullException("point");
            }
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            return points.Find(x => (x - point).GetLength() < 0.0001) != null;
        }
        public static Line CreateLine(this XYZ p1, XYZ p2)
        {
            return Line.CreateBound(p1, p2);
        }
        public static double SmallestAngleBetweenTwoVectors(XYZ v1, XYZ v2)
        {
            double value = 0;
            if (v1 != null && v2 != null)
            {
                value = Math.Abs(v1.AngleTo(v2));
                while (value > Math.PI * 0.5)
                {
                    value = Math.Abs(Math.PI - value);
                }
            }
            return value;
        }

        public static double DistancePoint2Line(this XYZ p, Line line)
        {
            XYZ endPoint = null;
            if (line.IsBound)
            {
                endPoint = line.GetEndPoint(0);
            }
            else
            {
                endPoint = line.Origin;
            }
            XYZ direction = line.Direction.Normalize();
            double d = Math.Abs((p - endPoint).DotProduct(direction));
            return Math.Sqrt(p.DistanceTo(endPoint) * p.DistanceTo(endPoint) - d * d);
        }

        public static XYZ ProjectPoint2Line(this XYZ p, Line line)
        {
            XYZ endPoint = line.GetEndPoint(0);
            XYZ vector1 = p - endPoint;
            XYZ direction = line.Direction.Normalize();
            return endPoint.Add(vector1.DotProduct(direction) * direction);
        }

        public static bool IsLeftSide(this XYZ point, Curve curve)
        {
            bool isLeft = false;
            if (point != null && curve != null)
            {
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);
                XYZ dir = end - start;
                XYZ v = point - start;
                double distance = curve.Project(point).Distance;
                double angle = dir.AngleOnPlaneTo(v, XYZ.BasisZ);
                if (distance < 0.01)
                {
                    throw new Exception("Point is coincided with curve!");
                }
                if (angle < Math.PI)
                {
                    isLeft = true;
                }
            }

            return isLeft;
        }

        public static bool IsVerticalUp(this XYZ vector)
        {
            return vector.Z.IsGreater(0);
        }

        public static bool IsVerticalDown(this XYZ vector)
        {
            return vector.Z.IsSmaller(0);
        }

        public static double AngleBetweenTwoVectors(this XYZ vectorOne, XYZ vectorTwo, bool absolute)
        {
            double num = vectorOne.X * vectorTwo.X + vectorOne.Y * vectorTwo.Y + vectorOne.Z * vectorTwo.Z;

            double num2 = VectorLength(vectorOne) * VectorLength(vectorTwo);

            if (absolute)
            {
                return Math.Acos(Math.Round(num / num2, 6)).RadiansToDegrees();
            }

            return Math.Acos(Math.Round(Math.Abs(num) / num2, 6)).RadiansToDegrees();
        }

        public static double VectorLength(XYZ vector)
        {
            return Math.Sqrt(Math.Pow(vector.X, 2.0) + Math.Pow(vector.Y, 2.0) + Math.Pow(vector.Z, 2.0));
        }

        public static bool IsEqual(this XYZ point1, XYZ point2)
        {
            double length = (point2 - point1).GetLength();
            return length < Constants.Eps;
        }

        public static bool IsNegative(this XYZ p, XYZ q)
        {
            return p.IsParallel(q) && p.DotProduct(q) < 0;
        }

        public static bool IsSameDirection(this XYZ p, XYZ q)
        {
            return p.IsParallel(q) && p.DotProduct(q) > 0;
        }

        public static XYZ FindXyzFromLengthVector(XYZ start, XYZ end, double leng)
        {
            XYZ value = null;
            var vector = (end - start).Normalize();
            value = start.Add(leng * vector);
            return value;
        }

        public static XYZ EditY(this XYZ p, double y)
        {
            return new XYZ(p.X, y, p.Z);
        }

        public static XYZ EditX(this XYZ p, double x)
        {
            return new XYZ(x, p.Y, p.Z);
        }

        public static bool IsPerpendicular(this XYZ v, XYZ w)
        {
            return 1E-09 < v.GetLength() && 1E-09 < w.GetLength() && 1E-09 > Math.Abs(v.DotProduct(w));
        }

        public static bool IsParallel(this XYZ p, XYZ q)
        {
            return p.CrossProduct(q).GetLength() < 0.01;
        }

        public static bool IsCodirectionalTo(this XYZ vecThis, XYZ vecTo)
        {
            if (vecTo == null)
            {
                throw new ArgumentNullException();
            }
            return Math.Abs(1.0 - vecThis.Normalize().DotProduct(vecTo.Normalize())) < 1E-06;
        }

        public static XYZ GetClosestPoint(this XYZ pt, List<XYZ> pts)
        {
            XYZ xyz = new XYZ();
            double num1 = 0.0;
            foreach (XYZ pt1 in pts)
            {
                if (!pt.Equals((object)pt1))
                {
                    double num2 = Math.Sqrt(Math.Pow(pt.X - pt1.X, 2.0) + Math.Pow(pt.Y - pt1.Y, 2.0) + Math.Pow(pt.Z - pt1.Z, 2.0));
                    if (xyz.IsZeroLength())
                    {
                        num1 = num2;
                        xyz = pt1;
                    }
                    else if (num2 < num1)
                    {
                        num1 = num2;
                        xyz = pt1;
                    }
                }
            }
            return xyz;
        }

        public static bool Iscontains(this XYZ point, List<XYZ> listPoint)
        {
            bool result = false;
            foreach (XYZ item in listPoint)
            {
                if (item.IsAlmostEqualTo(point))
                {
                    result = true;
                }
            }
            return result;
        }

        public static XYZ LastPointByDirection(this XYZ direction, List<XYZ> points)
        {
            var max = double.MinValue;
            var p = points.FirstOrDefault();
            foreach (var point in points)
            {
                var m = point.DotProduct(direction);
                if (m > max)
                {
                    max = m;
                    p = point;
                }
            }
            return p;
        }

        public static XYZ FirstPointByDirection(this XYZ direction, List<XYZ> points)
        {
            var min = double.MaxValue;
            var p = points.FirstOrDefault();
            foreach (var point in points)
            {
                var m = point.DotProduct(direction);
                if (m < min)
                {
                    min = m;
                    p = point;
                }
            }
            return p;
        }

        public static bool IsOppositeDirectionTo(this XYZ vecThis, XYZ vecTo)
        {
            return DoubleUtils.IsEqual(-1.0, vecThis.Normalize().DotProduct(vecTo.Normalize()));
        }

        public static bool IsOrthogonalTo(this XYZ vecThis, XYZ vecTo)
        {
            return DoubleUtils.IsEqual(0.0, vecThis.Normalize().DotProduct(vecTo.Normalize()));
        }

        public static bool IsHorizontal(this XYZ vecThis)
        {
            return vecThis.IsPerpendicular(XYZ.BasisZ);
        }

        public static bool IsHorizontal(this XYZ vecThis, View view)
        {
            return vecThis.IsPerpendicular(view.UpDirection);
        }

        public static bool IsVertical(this XYZ vecThis)
        {
            return vecThis.IsParallel(XYZ.BasisZ);
        }

        public static bool IsVertical(this XYZ vecThis, View view)
        {
            return vecThis.IsPerpendicular(view.RightDirection);
        }

        public static XYZ RotateRadians(this XYZ v, double radians)
        {
            var ca = Math.Cos(radians);
            var sa = Math.Sin(radians);
            return new XYZ(ca * v.X - sa * v.Y, sa * v.X + ca * v.Y, v.Z);
        }

        public static XYZ RotateDegree(this XYZ v, double degrees)
        {
            return v.RotateRadians(degrees * 0.0174532925);
        }

        public static XYZ GetClosestPt(XYZ pt, List<XYZ> pts)
        {
            XYZ xYZ = new XYZ();
            double num = 0.0;
            foreach (XYZ pt2 in pts)
            {
                if (!pt.Equals(pt2))
                {
                    double num2 = Math.Sqrt(Math.Pow(pt.X - pt2.X, 2.0) + Math.Pow(pt.Y - pt2.Y, 2.0) + Math.Pow(pt.Z - pt2.Z, 2.0));
                    if (xYZ.IsZeroLength())
                    {
                        num = num2;
                        xYZ = pt2;
                    }
                    else if (num2 < num)
                    {
                        num = num2;
                        xYZ = pt2;
                    }
                }
            }
            return xYZ;
        }

        public static XYZ Midpoint(this XYZ p, XYZ q)
        {
            return 0.5 * (p + q);
        }

        public static List<XYZ> GetExtremePoints(this List<XYZ> points)
        {
            double num2;
            double num = num2 = points[0].X;
            double num4;
            double num3 = num4 = points[0].Y;
            double num6;
            double num5 = num6 = points[0].Z;
            foreach (XYZ xyz in points)
            {
                num2 = Math.Min(xyz.X, num2);
                num4 = Math.Min(xyz.Y, num4);
                num6 = Math.Min(xyz.Z, num6);
                num = Math.Max(xyz.X, num);
                num3 = Math.Max(xyz.Y, num3);
                num5 = Math.Max(xyz.Z, num5);
            }
            XYZ item = new XYZ(num2, num4, num6);
            XYZ item2 = new XYZ(num, num3, num5);
            return new List<XYZ>
            {
                item,
                item2
            };
        }

        public static XYZ ModifyVector(this XYZ vector, double num, XYZEnum e)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            if (e == XYZEnum.X)
            {
                x = num;
            }
            if (e == XYZEnum.Y)
            {
                y = num;
            }
            if (e == XYZEnum.Z)
            {
                z = num;
            }
            return new XYZ(x, y, z);
        }

        public static XYZ EditZ(this XYZ p, double z)
        {
            return new XYZ(p.X, p.Y, z);
        }
        public static Line CreateLineByPointAndDirection(this XYZ p, XYZ direction)
        {
            return Line.CreateBound(p, p.Add(direction));
        }
        public static bool IsPointInsideLine(this XYZ C, Line line, double tol)
        {
            var A = line.GetEndPoint(0);
            var B = line.GetEndPoint(1);
            var AC = C - A;
            var AB = B - A;
            var BC = C - B;
            if (AC.IsAlmostEqualTo(XYZ.Zero, 0.001))
            {
                return true;
            }
            else
            {
                var cross = AC.CrossProduct(AB);
                if (cross.GetLength() < 0.001)
                {
                    if ((AC.GetLength() + BC.GetLength()).IsEqual(AB.GetLength(), tol))
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        public static Line LineByPoints(this XYZ sp, XYZ ep)
        {
            return Line.CreateBound(sp, ep);
        }
        public static bool IsOnCurve(this XYZ thisPoint, Curve curve)
        {
            if (thisPoint == null)
            {
                throw new ArgumentNullException("thisPoint");
            }
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }
            return curve.Distance(thisPoint) < 1E-05;
        }
    }
}