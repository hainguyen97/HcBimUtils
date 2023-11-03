using Autodesk.Revit.DB;


namespace HcBimUtils
{
    public static class LineUtils
    {
        public static Line Offset(this Line line, double offset, bool start, bool end)
        {
            XYZ startPoint = line.SP();
            XYZ endPoint = line.EP();
            XYZ direction = line.Direction;

            if (start)
            {
                startPoint = startPoint + direction * offset;
            }
            if (end)
            {
                endPoint = endPoint - direction * offset;
            }
            return Line.CreateBound(startPoint, endPoint);
        }
        public static bool IsParallelTo(this Line source, Line target)
        {
            return source.Direction.IsParallel(target.Direction);
        }

        public static bool IsPerpendicularTo(this Line source, Line target)
        {
            return source.Direction.IsPerpendicular(target.Direction);
        }
        public static Line ExtendLineBothEnd(this Line line, double num)
        {
            var sp = line.SP();
            var ep = line.EP();
            var direct = (ep - sp).Normalize();
            sp = sp.Add(direct * num * -1);
            ep = ep.Add(direct * num);
            return Line.CreateBound(sp, ep);
        }
        public static Line ProjectLine2Line(this Line l1, Line l2)
        {
            var a = l1.GetEndPoint(0);
            var b = l1.GetEndPoint(1);
            a = a.ProjectPoint2Line(l2);
            b = b.ProjectPoint2Line(l2);
            return Line.CreateBound(a, b);
        }
        public static bool IsAlmostInside(this Line l1, Line l2, double tol)
        {
            var flag = false;
            var p0 = l1.GetEndPoint(0).ProjectPoint2Line(l2);
            var p1 = l1.GetEndPoint(1).ProjectPoint2Line(l2);
            if (p0.IsPointInsideLine(l2, tol) && p1.IsPointInsideLine(l2, tol))
            {
                flag = true;
            }
            var p00 = l2.GetEndPoint(0).ProjectPoint2Line(l1);
            var p11 = l2.GetEndPoint(1).ProjectPoint2Line(l1);
            if (p00.IsPointInsideLine(l1, tol) && p11.IsPointInsideLine(l1, tol))
            {
                flag = true;
            }
            return flag;
        }
    }
}
