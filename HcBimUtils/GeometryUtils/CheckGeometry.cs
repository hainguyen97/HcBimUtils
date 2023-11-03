#region Namespaces

using System.IO;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

#endregion Namespaces

namespace HcBimUtils.GeometryUtils
{
    /// <summary>
    /// Kiểu dữ liệu chứa các phương thức tĩnh xử lý các đối tượng Revit
    /// </summary>
    public static class CheckGeometry
    {
        /// <summary>
        /// Lấy mặt phẳng của mặt đang xét
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <returns>Mặt phẳng trả về</returns>
        public static Plane GetPlane(PlanarFace f)
        {
            return Plane.CreateByOriginAndBasis(f.Origin, f.XVector, f.YVector);
        }

        /// <summary>
        /// Lấy mặt phẳng của mặt đang xét với trục Ox
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="vecX">Trục Ox của mặt phẳng</param>
        /// <returns></returns>
        public static Plane GetPlaneWithBasisX(PlanarFace f, XYZ vecX)
        {
            if (!GeometryUtils.IsEqual(GeometryUtils.DotMatrix(vecX, f.FaceNormal), 0)) throw new Exception("VecX is not perpendicular with Normal!");
            return Plane.CreateByOriginAndBasis(f.Origin, vecX, GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, f.FaceNormal)));
        }

        /// <summary>
        /// Lấy mặt phẳng của mặt đang xét với trục Oy
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="vecY">Trục Oy của mặt phẳng</param>
        /// <returns></returns>
        public static Plane GetPlaneWithBasisY(PlanarFace f, XYZ vecY)
        {
            if (!GeometryUtils.IsEqual(GeometryUtils.DotMatrix(vecY, f.FaceNormal), 0)) throw new Exception("VecY is not perpendicular with Normal!");
            return Plane.CreateByOriginAndBasis(f.Origin, GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecY, f.FaceNormal)), vecY);
        }

        /// <summary>
        /// Lấy tất cả các đường thẳng trong xác định mặt đang xét
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <returns></returns>
        public static List<Curve> GetCurves(PlanarFace f)
        {
            var curves = new List<Curve>();
            var curveLoops = f.GetEdgesAsCurveLoops();
            foreach (var cl in curveLoops)
            {
                curves.AddRange(cl);
                break;
            }
            return curves;
        }

        /// <summary>
        /// Lấy khoảng cách từ môt điểm đến một mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static double GetSignedDistance(Plane plane, XYZ point)
        {
            var v = point - plane.Origin;
            return Math.Abs(GeometryUtils.DotMatrix(plane.Normal, v));
        }

        /// <summary>
        /// Lấy khoảng cách từ một điểm đến một đường thẳng
        /// </summary>
        /// <param name="line">Đường thằng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static double GetSignedDistance(Line line, XYZ point)
        {
            if (IsPointInLineOrExtend(line, point)) return 0;
            return GeometryUtils.GetLength(point, GetProjectPoint(line, point));
        }

        /// <summary>
        /// Lấy khoảng cách từ một điểm đến một đường thẳng
        /// </summary>
        /// <param name="line">Đường thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static double GetSignedDistance(Curve line, XYZ point)
        {
            if (IsPointInLineOrExtend(ConvertLine(line), point)) return 0;
            return GeometryUtils.GetLength(point, GetProjectPoint(line, point));
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên đường thẳng
        /// </summary>
        /// <param name="line">Đường thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(Line line, XYZ point)
        {
            if (IsPointInLineOrExtend(line, point)) return point;
            var vecL = GeometryUtils.SubXYZ(line.GetEndPoint(1), line.GetEndPoint(0));
            var vecP = GeometryUtils.SubXYZ(point, line.GetEndPoint(0));
            var p = Plane.CreateByOriginAndBasis(line.GetEndPoint(0), GeometryUtils.UnitVector(vecL), GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecL, vecP)));
            return GetProjectPoint(p, point);
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên đường thẳng
        /// </summary>
        /// <param name="line">Đường thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(Curve line, XYZ point)
        {
            if (IsPointInLineOrExtend(ConvertLine(line), point)) return point;
            var vecL = GeometryUtils.SubXYZ(line.GetEndPoint(1), line.GetEndPoint(0));
            var vecP = GeometryUtils.SubXYZ(point, line.GetEndPoint(0));
            var p = Plane.CreateByOriginAndBasis(line.GetEndPoint(0), GeometryUtils.UnitVector(vecL), GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecL, vecP)));
            return GetProjectPoint(p, point);
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(Plane plane, XYZ point)
        {
            var d = GetSignedDistance(plane, point);
            var q = GeometryUtils.AddXYZ(point, GeometryUtils.MultiplyVector(plane.Normal, d));
            return IsPointInPlane(plane, q) ? q : GeometryUtils.AddXYZ(point, GeometryUtils.MultiplyVector(plane.Normal, -d));
        }

        /// <summary>
        /// Lấy điểm chiếu của một điểm lên mặt phẳng
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static XYZ GetProjectPoint(PlanarFace f, XYZ point)
        {
            var p = GetPlane(f);
            return GetProjectPoint(p, point);
        }

        /// <summary>
        /// Lấy đường thẳng chiếu của một đường thẳng lên mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="c">Đường thẳng đang xét</param>
        /// <returns></returns>
        public static Curve GetProjectLine(Plane plane, Curve c)
        {
            return Line.CreateBound(GetProjectPoint(plane, c.GetEndPoint(0)), GetProjectPoint(plane, c.GetEndPoint(1)));
        }

        /// <summary>
        /// Lấy polygon chiếu của một polygonn lên mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="polygon">Polygon đang xét</param>
        /// <returns></returns>
        public static Polygon GetProjectPolygon(Plane plane, Polygon polygon)
        {
            var cs = (from c in polygon.ListCurve select GetProjectLine(plane, c)).ToList();
            return new Polygon(cs);
        }

        /// <summary>
        /// Mô phỏng một tọa độ 3d lên hệ trục địa phương của mặt phẳng
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm 3d đang xét</param>
        /// <returns></returns>
        public static UV Evaluate(Plane plane, XYZ point)
        {
            if (!IsPointInPlane(plane, point)) point = GetProjectPoint(plane, point);
            var planeOx = Plane.CreateByOriginAndBasis(plane.Origin, plane.XVec, plane.Normal);
            var planeOy = Plane.CreateByOriginAndBasis(plane.Origin, plane.YVec, plane.Normal);
            var lenX = GetSignedDistance(planeOy, point);
            var lenY = GetSignedDistance(planeOx, point);
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var tLenX = lenX * Math.Pow(-1, i + 1);
                    var tLenY = lenY * Math.Pow(-1, j + 1);
                    var tPoint = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(plane.Origin, plane.XVec, tLenX), plane.YVec, tLenY);
                    if (GeometryUtils.IsEqual(tPoint, point)) return new UV(tLenX, tLenY);
                }
            }
            throw new Exception("Code compiler should never be here!");
        }

        /// <summary>
        /// Mô phỏng một tọa độ 2d trên mặt phẳng đang xét thành tọa độ 3d
        /// </summary>
        /// <param name="p">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm 2d đang xét</param>
        /// <returns></returns>
        public static XYZ Evaluate(Plane p, UV point)
        {
            var pnt = p.Origin;
            pnt = GeometryUtils.OffsetPoint(pnt, p.XVec, point.U);
            pnt = GeometryUtils.OffsetPoint(pnt, p.YVec, point.V);
            return pnt;
        }

        /// <summary>
        /// Mô phỏng một tọa độ 3d lên hệ trục địa phương của mặt đang xét
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="point">Điểm 3d đang xét</param>
        /// <returns></returns>
        public static UV Evaluate(PlanarFace f, XYZ point)
        {
            return Evaluate(GetPlane(f), point);
        }

        /// <summary>
        /// Mô phỏng một tọa độ 2d trên mặt đang xét thành tọa độ 3d
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        /// <param name="point">Điểm 2d đang xét</param>
        /// <returns></returns>
        public static XYZ Evaluate(PlanarFace f, UV point)
        {
            return f.Evaluate(point);
        }

        /// <summary>
        /// Mô phỏng một tọa độ 3d lên hệ trục địa phương của polygon đang xét
        /// </summary>
        /// <param name="f">Polygon đang xét</param>
        /// <param name="point">Điểm 3d đang xét</param>
        /// <returns></returns>
        public static UV Evaluate(Polygon f, XYZ point)
        {
            return Evaluate(GetPlane(f.Face), point);
        }

        /// <summary>
        /// Mô phỏng một tọa độ 2d trên polygon đang xét thành tọa độ 3d
        /// </summary>
        /// <param name="f">Polygon đang xét</param>
        /// <param name="point">Điểm 2d đang xét</param>
        /// <returns></returns>
        public static XYZ Evaluate(Polygon f, UV point)
        {
            return f.Face.Evaluate(point);
        }

        /// <summary>
        /// Kiểm tra một điểm nằm trên mặt phẳng hay không
        /// </summary>
        /// <param name="plane">Mặt phẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static bool IsPointInPlane(Plane plane, XYZ point)
        {
            return GeometryUtils.IsEqual(GetSignedDistance(plane, point), 0);
        }

        /// <summary>
        /// Kiểm tra một điểm 2d nằm trong polygon hay không
        /// </summary>
        /// <param name="p">Điểm 2d đang xét</param>
        /// <param name="polygon">Polygon đang xét</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(UV p, List<UV> polygon)
        {
            var minX = polygon[0].U;
            var maxX = polygon[0].U;
            var minY = polygon[0].V;
            var maxY = polygon[0].V;
            for (var i = 1; i < polygon.Count; i++)
            {
                var q = polygon[i];
                minX = Math.Min(q.U, minX);
                maxX = Math.Max(q.U, maxX);
                minY = Math.Min(q.V, minY);
                maxY = Math.Max(q.V, maxY);
            }

            if (p.U < minX || p.U > maxX || p.V < minY || p.V > maxY)
            {
                return false;
            }
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].V > p.V) != (polygon[j].V > p.V) &&
                     p.U < (polygon[j].U - polygon[i].U) * (p.V - polygon[i].V) / (polygon[j].V - polygon[i].V) + polygon[i].U)
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        /// <summary>
        /// Kiểm tra một điểm nằm trên đoạn thẳng hay không
        /// </summary>
        /// <param name="line">Đoạn thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static bool IsPointInLine(Line line, XYZ point)
        {
            if (GeometryUtils.IsEqual(point, line.GetEndPoint(0)) || GeometryUtils.IsEqual(point, line.GetEndPoint(1))) return true;
            return GeometryUtils.IsOppositeDirection(GeometryUtils.SubXYZ(point, line.GetEndPoint(0)), GeometryUtils.SubXYZ(point, line.GetEndPoint(1)));
        }

        /// <summary>
        /// Kiểm tra một điểm nằm trên phần kéo dài của đoạn thẳng hay không
        /// </summary>
        /// <param name="line">Đoạn thẳng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static bool IsPointInLineExtend(Line line, XYZ point)
        {
            if (GeometryUtils.IsEqual(point, line.GetEndPoint(0)) || GeometryUtils.IsEqual(point, line.GetEndPoint(1))) return true;
            return GeometryUtils.IsSameDirection(GeometryUtils.SubXYZ(point, line.GetEndPoint(0)), GeometryUtils.SubXYZ(point, line.GetEndPoint(1)));
        }

        /// <summary>
        /// Kiểm tra một điểm nằm trên đường thẳng hay không
        /// </summary>
        /// <param name="line">Đường thằng đang xét</param>
        /// <param name="point">Điểm đang xét</param>
        /// <returns></returns>
        public static bool IsPointInLineOrExtend(Line line, XYZ point)
        {
            if (GeometryUtils.IsEqual(point, line.GetEndPoint(0)) || GeometryUtils.IsEqual(point, line.GetEndPoint(1))) return true;
            return GeometryUtils.IsSameOrOppositeDirection(GeometryUtils.SubXYZ(point, line.GetEndPoint(0)), GeometryUtils.SubXYZ(point, line.GetEndPoint(1)));
        }

        /// <summary>
        /// Hàm kiểm tra tính tương đối giữa một điểm 2d và một polygon trả về một enum.
        /// Các kết quả trả về là điểm, cạnh, bên trong, bên ngoài polygon.
        /// </summary>
        /// <param name="p">Điểm đang xét</param>
        /// <param name="polygon">Polgyon đang xét</param>
        /// <returns></returns>
        public static PointComparePolygonResult PointComparePolygon(UV p, List<UV> polygon)
        {
            var check1 = IsPointInPolygon(p, polygon);
            for (var i = 0; i < polygon.Count; i++)
            {
                if (GeometryUtils.IsEqual(p, polygon[i])) return PointComparePolygonResult.Node;

                var vec1 = GeometryUtils.SubXYZ(p, polygon[i]);
                UV vec2;
                if (i != polygon.Count - 1)
                {
                    if (GeometryUtils.IsEqual(p, polygon[i + 1])) continue;
                    vec2 = GeometryUtils.SubXYZ(p, polygon[i + 1]);
                }
                else
                {
                    if (GeometryUtils.IsEqual(p, polygon[0])) continue;
                    vec2 = GeometryUtils.SubXYZ(p, polygon[0]);
                }
                if (GeometryUtils.IsOppositeDirection(vec1, vec2)) return PointComparePolygonResult.Boundary;
            }
            return check1 ? PointComparePolygonResult.Inside : PointComparePolygonResult.Outside;
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu curve đang line
        /// </summary>
        /// <param name="c">Kiểu dữ liệu Curve đang xét</param>
        /// <returns></returns>
        public static Line ConvertLine(Curve c)
        {
            return Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1));
        }

        /// <summary>
        /// Láy vector chỉ phương của đường thẳng
        /// </summary>
        /// <param name="c">Đường thẳng đang xét</param>
        /// <returns></returns>
        public static XYZ GetDirection(Curve c)
        {
            return GeometryUtils.UnitVector(GeometryUtils.SubXYZ(c.GetEndPoint(1), c.GetEndPoint(0)));
        }

        /// <summary>
        /// Tạo một DetailLine của đoạn thẳng trên view đang xét
        /// </summary>
        /// <param name="c">Đoạn thẳng đang xét</param>
        /// <param name="doc">Document đang xét</param>
        /// <param name="v">View đang xét</param>
        public static void CreateDetailLine(Curve c, Document doc, View v)
        {
            var dl = doc.Create.NewDetailCurve(v, c) as DetailLine;
        }

        public static void CreateDetailLinePolygon(Polygon pl, Document doc, View v)
        {
            foreach (var c in pl.ListCurve)
            {
                CreateDetailLine(c, doc, v);
            }
        }

        /// <summary>
        /// Tạo thành một polygon có các cạnh liên kết được với nhau thành một khối nằm trong list các đoạn thẳng cho trước
        /// True: tạo ra polygon thỏa các điều kiện
        /// False: không thỏa được các điều kiện, trả về null
        /// </summary>
        /// <param name="listCurve">List các đoạn thẳng cho trước</param>
        /// <param name="pls">Polygon được trả về thỏa các điều kiện</param>
        /// <returns></returns>
        public static bool CreateListPolygon(List<Curve> listCurve, out List<Polygon> pls)
        {
            pls = new List<Polygon>();
            foreach (var c in listCurve)
            {
                var cs = new List<Curve>();
                cs.Add(Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1)));
                var i = 0; var check = true;
                while (!GeometryUtils.IsEqual(cs[0].GetEndPoint(0), cs[cs.Count - 1].GetEndPoint(1)))
                {
                    i++;
                    foreach (var c1 in listCurve)
                    {
                        var pnt = cs[cs.Count - 1].GetEndPoint(1);
                        var prePnt = cs[cs.Count - 1].GetEndPoint(0);
                        if (GeometryUtils.IsEqual(pnt, c1.GetEndPoint(0)))
                        {
                            if (GeometryUtils.IsEqual(prePnt, c1.GetEndPoint(1)))
                            {
                                continue;
                            }
                            cs.Add(Line.CreateBound(c1.GetEndPoint(0), c1.GetEndPoint(1)));
                            break;
                        }

                        if (GeometryUtils.IsEqual(pnt, c1.GetEndPoint(1)))
                        {
                            if (GeometryUtils.IsEqual(prePnt, c1.GetEndPoint(0)))
                            {
                                continue;
                            }
                            cs.Add(Line.CreateBound(c1.GetEndPoint(1), c1.GetEndPoint(0)));
                            break;
                        }
                    }

                    if (i != 200) continue;
                    check = false; break;
                }

                if (!check) continue;
                var plgon = new Polygon(cs);

                if (pls.Count == 0) pls.Add(plgon);
                else
                {
                    if (pls.Any(pl => pl == plgon))
                    {
                        check = false;
                    }

                    if (check) pls.Add(plgon);
                }
            }
            return pls.Count != 0;
        }

        /// <summary>
        /// Tạo ra một polygon từ một mặt của tham biến family instance nhập vào
        /// Điều kiện đúng khi family instance thỏa mãn các điều kiện nhất định
        /// </summary>
        /// <param name="fi">Family Instance đang xét</param>
        /// <returns></returns>
        public static Polygon GetPolygonFromFaceFamilyInstance(FamilyInstance fi)
        {
            var geoElem = fi.get_Geometry(new Options { ComputeReferences = true });
            var cs = (from geoIns in geoElem.OfType<GeometryInstance>() let tf = geoIns.Transform from geoSymObj in geoIns.GetSymbolGeometry() let c = geoSymObj as Line where c != null select GeometryUtils.TransformCurve(c, tf)).ToList();
            if (cs.Count < 3) throw new Exception("Incorrect input curve!");
            return new Polygon(cs);
        }

        /// <summary>
        /// Chuyển đổi kiểu dữ liệu từ string sang XYZ
        /// Giá trị nhập vào phải thỏa các điều kiện nhất định
        /// </summary>
        /// <param name="pointString">Giá trị string đang xét</param>
        /// <returns></returns>
        public static XYZ ConvertStringToXyz(string pointString)
        {
            var nums = new List<double>();
            foreach (var s in pointString.Split('(', ',', ' ', ')'))
            {
                if (double.TryParse(s, out var x)) { nums.Add(x); }
            }
            return new XYZ(nums[0], nums[1], nums[2]);
        }

        /// <summary>
        /// Tạo và trả về mặt cắt cho đối tượng tường đang xét
        /// </summary>
        /// <param name="linkedDoc">Document chứa đối tượng tường đang xét</param>
        /// <param name="doc">Document liên kết</param>
        /// <param name="id">Địa chỉ Id của đối tượng tường đang xét</param>
        /// <param name="viewName">Tên của mặt cắt trả về</param>
        /// <param name="offset">Giá trị offset từ biên đối tượng tường</param>
        /// <returns></returns>
        public static ViewSection CreateWallSection(Document linkedDoc, Document doc, ElementId id, string viewName, double offset)
        {
            var e = linkedDoc.GetElement(id);
            if (e is not Wall wall) throw new Exception("Element is not a wall!");
            var line = (wall.Location as LocationCurve)?.Curve as Line;

            ViewFamilyType vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

            XYZ p1 = line.GetEndPoint(0), p2 = line.GetEndPoint(1);
            List<XYZ> ps = new List<XYZ> { p1, p2 }; ps.Sort(new ZyxComparer());
            p1 = ps[0]; p2 = ps[1];

            var bb = wall.get_BoundingBox(null);
            double minZ = bb.Min.Z, maxZ = bb.Max.Z;

            var l = GeometryUtils.GetLength(GeometryUtils.SubXYZ(p2, p1));
            var h = maxZ - minZ;
            var w = wall.WallType.Width;

            var min = new XYZ(-l / 2 - offset, minZ - offset, -w - offset);
            var max = new XYZ(l / 2 + offset, maxZ + offset, w + offset);

            var tf = Transform.Identity;
            tf.Origin = (p1 + p2) / 2;
            tf.BasisX = GeometryUtils.UnitVector(p1 - p2);
            tf.BasisY = XYZ.BasisZ;
            tf.BasisZ = GeometryUtils.CrossMatrix(tf.BasisX, tf.BasisY);

            var sectionBox = new BoundingBoxXYZ() { Transform = tf, Min = min, Max = max };
            var vs = ViewSection.CreateSection(doc, vft.Id, sectionBox);

            var wallDir = GeometryUtils.UnitVector(p2 - p1);
            var upDir = XYZ.BasisZ;
            var viewDir = GeometryUtils.CrossMatrix(wallDir, upDir);

            min = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p1, -wallDir, offset), -viewDir, offset);
            min = new XYZ(min.X, min.Y, minZ - offset);
            max = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p2, wallDir, offset), viewDir, offset);
            max = new XYZ(max.X, max.Y, maxZ + offset);

            tf = vs.get_BoundingBox(null).Transform.Inverse;
            max = tf.OfPoint(max);
            min = tf.OfPoint(min);
            double maxx, maxy, maxz, minx, miny, minz;
            if (max.Z > min.Z)
            {
                maxz = max.Z;
                minz = min.Z;
            }
            else
            {
                maxz = min.Z;
                minz = max.Z;
            }

            if (Math.Round(max.X, 4).Equals(Math.Round(min.X, 4)))
            {
                maxx = max.X;
                minx = minz;
            }
            else if (max.X > min.X)
            {
                maxx = max.X;
                minx = min.X;
            }
            else
            {
                maxx = min.X;
                minx = max.X;
            }

            if (Math.Round(max.Y, 4).Equals(Math.Round(min.Y, 4)))
            {
                maxy = max.Y;
                miny = minz;
            }
            else if (max.Y > min.Y)
            {
                maxy = max.Y;
                miny = min.Y;
            }
            else
            {
                maxy = min.Y;
                miny = max.Y;
            }

            BoundingBoxXYZ sectionView = new BoundingBoxXYZ();
            sectionView.Max = new XYZ(maxx, maxy, maxz);
            sectionView.Min = new XYZ(minx, miny, minz);

            vs.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(ElementId.InvalidElementId);

            vs.get_Parameter(BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING).Set(0);

            vs.CropBoxActive = true;
            vs.CropBoxVisible = true;

            doc.Regenerate();

            vs.CropBox = sectionView;
            vs.Name = viewName;
            return vs;
        }

        /// <summary>
        /// Tạo và trả về mặt cắt cho đối tượng tường đang xét với phương nhìn mặt cắt phụ thuộc vào polygon định nghĩa đường bao tất cả các chân tường tham gia
        /// </summary>
        /// <param name="linkedDoc">Document chứa đối tượng tường đang xét</param>
        /// <param name="doc">Document liên kết</param>
        /// <param name="directPolygon">Polygon để xác định phương nhìn của mặt cắt</param>
        /// <param name="id">Địa chỉ Id của đối tượng tường đang xét</param>
        /// <param name="viewName">Tên của mặt cắt trả về</param>
        /// <param name="offset">Giá trị offset từ biên đối tượng tường</param>
        /// <returns></returns>
        public static ViewSection CreateWallSection(Document linkedDoc, Document doc, Polygon directPolygon, ElementId id, string viewName, double offset)
        {
            var e = linkedDoc.GetElement(id);
            if (e is not Wall wall) throw new Exception("Element is not a wall!");
            var line = (wall.Location as LocationCurve).Curve as Line;

            ViewFamilyType vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.Section == x.ViewFamily);

            XYZ p1 = line.GetEndPoint(0), p2 = line.GetEndPoint(1);
            var ps = new List<XYZ> { p1, p2 }; ps.Sort(new ZyxComparer());
            p1 = ps[0]; p2 = ps[1];

            var bb = wall.get_BoundingBox(null);
            double minZ = bb.Min.Z, maxZ = bb.Max.Z;

            var l = GeometryUtils.GetLength(GeometryUtils.SubXYZ(p2, p1));
            var h = maxZ - minZ;
            var w = wall.WallType.Width;

            var tfMin = new XYZ(-l / 2 - offset, minZ - offset, -w - offset);
            var tfMax = new XYZ(l / 2 + offset, maxZ + offset, w + offset);

            var wallDir = GeometryUtils.UnitVector(p2 - p1);
            var upDir = XYZ.BasisZ;
            var viewDir = GeometryUtils.CrossMatrix(wallDir, upDir);

            var midPoint = (p1 + p2) / 2;
            var pMidPoint = GetProjectPoint(directPolygon.Plane, midPoint);

            var pPnt = GeometryUtils.OffsetPoint(pMidPoint, viewDir, w * 10);
            if (GeometryUtils.IsBigger(GeometryUtils.GetLength(pMidPoint, directPolygon.CentralXYZPoint), GeometryUtils.GetLength(pPnt, directPolygon.CentralXYZPoint)))
            {
                wallDir = -wallDir;
                upDir = XYZ.BasisZ;
                viewDir = GeometryUtils.CrossMatrix(wallDir, upDir);
            }

            pPnt = GeometryUtils.OffsetPoint(p1, wallDir, offset);
            XYZ min, max;
            if (GeometryUtils.IsBigger(GeometryUtils.GetLength(GeometryUtils.SubXYZ(pPnt, midPoint)), GeometryUtils.GetLength(GeometryUtils.SubXYZ(p1, midPoint))))
            {
                min = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p1, wallDir, offset), -viewDir, offset);
                max = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p2, -wallDir, offset), viewDir, offset);
            }
            else
            {
                min = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p1, -wallDir, offset), -viewDir, offset);
                max = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(p2, wallDir, offset), viewDir, offset);
            }
            min = new XYZ(min.X, min.Y, minZ - offset);
            max = new XYZ(max.X, max.Y, maxZ + offset);

            var tf = Transform.Identity;
            tf.Origin = (p1 + p2) / 2;
            tf.BasisX = wallDir;
            tf.BasisY = XYZ.BasisZ;
            tf.BasisZ = GeometryUtils.CrossMatrix(wallDir, upDir);

            var sectionBox = new BoundingBoxXYZ() { Transform = tf, Min = tfMin, Max = tfMax };
            var vs = ViewSection.CreateSection(doc, vft.Id, sectionBox);

            tf = vs.get_BoundingBox(null).Transform.Inverse;
            max = tf.OfPoint(max);
            min = tf.OfPoint(min);
            double maxx, maxy, maxz, minx, miny, minz;
            if (max.Z > min.Z)
            {
                maxz = max.Z;
                minz = min.Z;
            }
            else
            {
                maxz = min.Z;
                minz = max.Z;
            }

            if (Math.Round(max.X, 4) == Math.Round(min.X, 4))
            {
                maxx = max.X;
                minx = minz;
            }
            else if (max.X > min.X)
            {
                maxx = max.X;
                minx = min.X;
            }
            else
            {
                maxx = min.X;
                minx = max.X;
            }

            if (Math.Round(max.Y, 4) == Math.Round(min.Y, 4))
            {
                maxy = max.Y;
                miny = minz;
            }
            else if (max.Y > min.Y)
            {
                maxy = max.Y;
                miny = min.Y;
            }
            else
            {
                maxy = min.Y;
                miny = max.Y;
            }

            BoundingBoxXYZ sectionView = new BoundingBoxXYZ();
            sectionView.Max = new XYZ(maxx, maxy, maxz);
            sectionView.Min = new XYZ(minx, miny, minz);

            vs.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(ElementId.InvalidElementId);

            vs.get_Parameter(BuiltInParameter.VIEWER_BOUND_FAR_CLIPPING).Set(0);

            vs.CropBoxActive = true;
            vs.CropBoxVisible = true;

            doc.Regenerate();

            vs.CropBox = sectionView;
            vs.Name = viewName;
            return vs;
        }

        /// <summary>
        /// Tạo và trả về mặt cắt callout cho đối tượng sàn đang xét
        /// </summary>
        /// <param name="doc">Document chứa đối tượng sàn đang xét</param>
        /// <param name="views">List tất cả các view trong mô hình</param>
        /// <param name="level">Tên level chưa đối tượng sàn</param>
        /// <param name="bb">BoundingBox của đối tượng sàn</param>
        /// <param name="viewName">Tên mặt cắt trả về</param>
        /// <param name="offset">Giá trị offset từ biên đối tượng sàn</param>
        /// <returns></returns>
        public static View CreateFloorCallout(Document doc, List<View> views, string level, BoundingBoxXYZ bb, string viewName, double offset)
        {
            var vft = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>().FirstOrDefault<ViewFamilyType>(x => ViewFamily.FloorPlan == x.ViewFamily);
            var max = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(bb.Max, XYZ.BasisX, offset), XYZ.BasisY, offset), XYZ.BasisZ, offset);
            var min = GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(GeometryUtils.OffsetPoint(bb.Min, -XYZ.BasisX, offset), -XYZ.BasisY, offset), -XYZ.BasisZ, offset);
            bb = new BoundingBoxXYZ { Max = max, Min = min };
            View pv = null;
            bool check = false;
            foreach (var v in views)
            {
                try
                {
                    var s = v.LookupParameter("Associated Level").AsString();
                    if (s != level) continue;
                    pv = v; check = true; break;
                }
                catch
                {
                    // ignored
                }
            }
            if (!check) throw new Exception("Invalid level name!");
            var vs = ViewSection.CreateCallout(doc, pv.Id, vft.Id, min, max);
            vs.CropBox = bb;
            vs.Name = viewName;
            return vs;
        }

        /// <summary>
        /// Trả về đường dẫn thư mục chứa document đang xét
        /// </summary>
        /// <param name="doc">Document đang xét</param>
        /// <returns></returns>
        public static string GetDirectoryPath(Document doc)
        {
            return Path.GetDirectoryName(doc.PathName);
        }

        /// <summary>
        /// Trả về đường dẫn thư mục chưa document đang xét
        /// </summary>
        /// <param name="documentName">Tên document đang xét</param>
        /// <returns></returns>
        public static string GetDirectoryPath(string documentName)
        {
            return Path.GetDirectoryName(documentName);
        }

        /// <summary>
        /// Trả về đường dẫn mới chưa đường dẫn document đang xét, thêm vào tên và đuôi mở rộng mới
        /// </summary>
        /// <param name="doc">Document đang xét</param>
        /// <param name="name">Tên mới được thêm vào</param>
        /// <param name="exten">Đuôi mở rộng mới</param>
        /// <returns></returns>
        public static string CreateNameWithDocumentPathName(Document doc, string name, string exten)
        {
            var s = GetDirectoryPath(doc);
            var s1 = doc.PathName.Substring(s.Length + 1);
            return Path.Combine(s, s1.Substring(0, s1.Length - 4) + name + "." + exten);
        }

        /// <summary>
        /// Trả về đường dẫn mới chưa đường dẫn document đang xét, thêm vào tên và đuôi mở rộng mới
        /// </summary>
        /// <param name="documentName">Tên document đang xét</param>
        /// <param name="name">Tên mới được thêm vào</param>
        /// <param name="exten">Đuôi mở rộng mới</param>
        /// <returns></returns>
        public static string CreateNameWithDocumentPathName(string documentName, string name, string exten)
        {
            var s = GetDirectoryPath(documentName);
            var s1 = documentName.Substring(s.Length + 1);
            return Path.Combine(s, s1.Substring(0, s1.Length - 4) + name + "." + exten);
        }

        /// <summary>
        /// Kiểm tra file trong đường dẫn đang được sử dụng hay không
        /// </summary>
        /// <param name="path">Đường dẫn file</param>
        /// <returns></returns>
        public static bool IsFileInUse(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("'path' cannot be null or empty.", "path");

            try
            {
                using (new FileStream(path, FileMode.Open, FileAccess.Read)) { }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu BoundingBoxXYZ sang kiểu dữ liệu string
        /// </summary>
        /// <param name="bb">BoundingBoxXYZ đang xét</param>
        /// <returns></returns>
        public static string ConvertBoundingBoxToString(BoundingBoxXYZ bb)
        {
            return "{" + bb.Min + ";" + bb.Max + "}";
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu string sang kiểu dữ liệu BoundingBoxXYZ
        /// </summary>
        /// <param name="bbString">Giá trị string đang xét phải thỏa các điều kiện nhất định</param>
        /// <returns></returns>
        public static BoundingBoxXYZ ConvertStringToBoundingBox(string bbString)
        {
            var bb = new BoundingBoxXYZ();
            var ss = bbString.Split(';');
            ss[0] = ss[0].Substring(1, ss[0].Length - 1); ss[1] = ss[1].Substring(0, ss[1].Length - 1);
            bb.Min = ConvertStringToXyz(ss[0]);
            bb.Max = ConvertStringToXyz(ss[1]);
            return bb;
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu Polygon sang kiểu dữ liệu string
        /// </summary>
        /// <param name="plgon">Polygon đang xét</param>
        /// <returns></returns>
        public static string ConvertPolygonToString(Polygon plgon)
        {
            var s = "{";
            for (var i = 0; i < plgon.ListXYZPoint.Count; i++)
            {
                if (i != plgon.ListXYZPoint.Count - 1)
                {
                    s += plgon.ListXYZPoint[i] + ";";
                }
                else
                {
                    s += plgon.ListXYZPoint[i] + "}";
                }
            }
            return s;
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu string sang kiểu dữ liệu polygon
        /// </summary>
        /// <param name="bbString">Giá trị string đang xét phải thỏa các điều kiện nhất định</param>
        /// <returns></returns>
        public static Polygon ConvertStringToPolygon(string bbString)
        {
            var ss = bbString.Split(';');
            ss[0] = ss[0].Substring(1, ss[0].Length - 1);
            ss[ss.Length - 1] = ss[ss.Length - 1].Substring(0, ss[ss.Length - 1].Length - 1);
            var points = (from s in ss select ConvertStringToXyz(s)).ToList();
            return new Polygon(points);
        }

        /// <summary>
        /// Chuyển đổi từ kiểu dữ liệu CurveLoop sang List<Curve>
        /// </summary>
        /// <param name="cl">CurveLoop đang xét</param>
        /// <returns></returns>
        public static List<Curve> ConvertCurveLoopToCurveList(CurveLoop cl)
        {
            return cl.ToList();
        }

        /// <summary>
        /// Tạo ra một polygon là kết quả của việc cắt một polygon bằng một polygon
        /// *Không nên xài hàm này do liên quan tới các điều kiện phức tạp
        /// </summary>
        /// <param name="mainPolygon">Polygon bị cắt</param>
        /// <param name="secPolygon">Polygon cắt</param>
        /// <returns></returns>
        public static Polygon GetPolygonCut(Polygon mainPolygon, Polygon secPolygon)
        {
            var res = new PolygonComparePolygonResult(mainPolygon, secPolygon);
            if (res.ListPolygon[0] != secPolygon) throw new Exception("Secondary Polygon must inside Main Polygon!");
            var cs = new List<Curve>();
            foreach (var c in secPolygon.ListCurve)
            {
                var lpRes = new LineComparePolygonResult(mainPolygon, ConvertLine(c));
                if (lpRes.Type == LineComparePolygonType.Inside)
                {
                    foreach (var _ in mainPolygon.ListCurve.Select(c1 => new LineCompareLineResult(c, c1)).Where(llres => llres.Type == LineCompareLineType.SameDirectionLineOverlap))
                    {
                        goto Here;
                    }

                    cs.Add(c);
                }
            Here:;
            }
            var isInside = false;
            foreach (var c in mainPolygon.ListCurve)
            {
                var lpRes = new LineComparePolygonResult(secPolygon, ConvertLine(c));
                if (lpRes.Type == LineComparePolygonType.Outside)
                {
                    cs.Add(c);
                    continue;
                }
                foreach (var llRes in secPolygon.ListCurve.Select(c1 => new LineCompareLineResult(c, c1)).Where(llRes => llRes.Type == LineCompareLineType.SameDirectionLineOverlap))
                {
                    if (llRes.ListOuterLine.Count == 0) break;
                    cs.AddRange((from l in llRes.ListOuterLine let lpRes1 = new LineComparePolygonResult(secPolygon, l) where lpRes1.Type != LineComparePolygonType.Inside select l).Cast<Curve>());
                    break;
                }
            }
            if (isInside) throw new Exception("Secondary Polygon must be tangential with Main Polygon!");
            return new Polygon(cs);
        }

        /// <summary>
        /// Tạo ra một polygon(chuyển đổi về kiểu dữ liệu List<Curve>) là kết quả của việc cắt một polygon bằng một polygon
        /// *Không nên xài hàm này do liên quan tới các điều kiện phức tạp
        /// </summary>
        /// <param name="mainPolygon">Polygon bị cắt</param>
        /// <param name="secPolygon">Polygon cắt</param>
        /// <returns></returns>
        public static List<Curve> GetCurvesCut(Polygon mainPolygon, Polygon secPolygon)
        {
            var res = new PolygonComparePolygonResult(mainPolygon, secPolygon);
            if (res.ListPolygon[0] != secPolygon) throw new Exception("Secondary Polygon must inside Main Polygon!");
            var cs = new List<Curve>();
            foreach (var c in secPolygon.ListCurve)
            {
                var lpRes = new LineComparePolygonResult(mainPolygon, ConvertLine(c));
                if (lpRes.Type == LineComparePolygonType.Inside)
                {
                    foreach (var _ in mainPolygon.ListCurve.Select(c1 => new LineCompareLineResult(c, c1)).Where(llres => llres.Type == LineCompareLineType.SameDirectionLineOverlap))
                    {
                        goto Here;
                    }

                    cs.Add(c);
                }
            Here:;
            }
            var isInside = false;
            foreach (var c in mainPolygon.ListCurve)
            {
                var lpRes = new LineComparePolygonResult(secPolygon, ConvertLine(c));
                if (lpRes.Type == LineComparePolygonType.Outside)
                {
                    cs.Add(c);
                    continue;
                }
                foreach (var llRes in secPolygon.ListCurve.Select(c1 => new LineCompareLineResult(c, c1)).Where(llRes => llRes.Type == LineCompareLineType.SameDirectionLineOverlap))
                {
                    if (llRes.ListOuterLine.Count == 0) break;
                    cs.AddRange((from l in llRes.ListOuterLine let lpRes1 = new LineComparePolygonResult(secPolygon, l) where lpRes1.Type != LineComparePolygonType.Inside select l).Cast<Curve>());
                    break;
                }
            }
            if (isInside) throw new Exception("Secondary Polygon must be tangential with Main Polygon!");
            return cs;
        }

        /// <summary>
        /// Trả về hệ tọa độ của đối tượng trong document hiện tại
        /// Đối tượng kiểm tra nên là Family Instance (bị thay đổi hệ tọa độ)
        /// </summary>
        /// <param name="e">Đối tượng Revit đang xét</param>
        /// <returns></returns>
        public static Transform GetTransform(Element e)
        {
            var geoEle = e.get_Geometry(new Options() { ComputeReferences = true });
            return (from geoIns in geoEle.OfType<GeometryInstance>() where geoIns.Transform != null select geoIns.Transform).FirstOrDefault();
        }

        /// <summary>
        /// Đặt một Detail Item trong một view xác định trong document đang xét
        /// </summary>
        /// <param name="familyName">Tên family của Detail Item đang xét</param>
        /// <param name="location">Điểm đặt trong view đang xét</param>
        /// <param name="doc">Document đang xét</param>
        /// <param name="tx">Transaction đang xét</param>
        /// <param name="v">View đang xét</param>
        /// <param name="property_Values">Thiết lập các thuộc tính liên quan theo thứ tự: [Tên thuộc tính], [Giá trị thuộc tính], ...</param>
        public static void InsertDetailItem(string familyName, XYZ location, Document doc, Transaction tx, View v, params string[] property_Values)
        {
            Family f = null;
            var col = new FilteredElementCollector(doc).OfClass(typeof(Family));
            var check = false;
            foreach (var e in col)
            {
                if (e.Name != familyName) continue;
                f = (Family)e;
                check = true;
                break;
            }
            if (!check)
            {
                var filePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath;
                var directoryPath = Path.GetDirectoryName(filePath);
                if (directoryPath != null)
                {
                    var fullFamilyName = Path.Combine(directoryPath, familyName + ".rfa");
                    doc.LoadFamily(fullFamilyName, out f);
                }
            }

            FamilySymbol symbol = null;
            if (f != null)
                foreach (var symbolId in f.GetFamilySymbolIds())
                {
                    symbol = doc.GetElement(symbolId) as FamilySymbol;
                    if (symbol is { IsActive: false })
                    {
                        symbol.Activate();
                    }

                    break;
                }

            var fi = doc.Create.NewFamilyInstance(location, symbol, v);

            for (var i = 0; i < property_Values.Length; i += 2)
            {
                fi.LookupParameter(property_Values[i]).Set(property_Values[i + 1]);
            }
        }

        /// <summary>
        /// Show một task dialog chứa các thông tin theo format định sẵn
        /// </summary>
        /// <param name="contents">Thiết lập các thông tin liên quan theo thứ tự: [Tên thuộc tính]:   [Giá trị thuộc tính] ...</param>
        public static void ShowTaskDialog(params string[] contents)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < contents.Length; i += 2)
            {
                if (i + 1 <= contents.Length - 1)
                {
                    sb.Append(contents[i] + ":" + " \t" + contents[i + 1] + "\n");
                }
                else
                {
                    sb.Append(contents[i]);
                }
            }
            TaskDialog.Show("Revit", sb.ToString());
        }

        /// <summary>
        /// Tạo ra các model line là các cạnh của polygon đang xét
        /// </summary>
        /// <param name="doc">Document đang xét</param>
        /// <param name="pl">Polygon đang xét</param>
        public static void CreateModelLinePolygon(Document doc, Polygon pl)
        {
            var sp = SketchPlane.Create(doc, pl.Plane);
            foreach (var c in pl.ListCurve)
            {
                CreateModelLine(doc, sp, c);
            }
        }

        /// <summary>
        /// Tạo ra model từ đoạn thẳng đang xét
        /// </summary>
        /// <param name="doc">Document đang xét</param>
        /// <param name="sp">Mặt phẳng làm việc sẽ chứa đoạn thẳng</param>
        /// <param name="c">Đoạn thẳng đang xét</param>
        public static void CreateModelLine(Document doc, SketchPlane sp, Curve c)
        {
            while (true)
            {
                if (sp != null)
                {
                    doc.Create.NewModelCurve(c, sp);
                    return;
                }

                if (GeometryUtils.IsSameOrOppositeDirection(GetDirection(c), XYZ.BasisZ))
                {
                    sp = SketchPlane.Create(doc, Plane.CreateByOriginAndBasis(c.GetEndPoint(0), XYZ.BasisZ, XYZ.BasisX));
                    continue;
                }

                var vecY = GetDirection(c).CrossProduct(XYZ.BasisZ);
                sp = SketchPlane.Create(doc, Plane.CreateByOriginAndBasis(c.GetEndPoint(0), GeometryUtils.UnitVector(GetDirection(c)), GeometryUtils.UnitVector(vecY)));
            }
        }

        /// <summary>
        /// Offset một polygon theo một khoảng cách nhất định
        /// </summary>
        /// <param name="polygon">Polygon đang xét</param>
        /// <param name="distance">Khoảng cách</param>
        /// <param name="isInside">True: offset vào trong, False: offset ra ngoài</param>
        /// <returns></returns>
        public static Polygon OffsetPolygon(Polygon polygon, double distance, bool isInside = true)
        {
            var points = new List<XYZ>();
            for (var i = 0; i < polygon.ListXYZPoint.Count; i++)
            {
                XYZ vec;
                if (i == 0)
                {
                    Curve c1 = Line.CreateBound(polygon.ListXYZPoint[polygon.ListXYZPoint.Count - 1], polygon.ListXYZPoint[i]);
                    Curve c2 = Line.CreateBound(polygon.ListXYZPoint[i], polygon.ListXYZPoint[i + 1]);

                    vec = polygon.Normal.CrossProduct(GetDirection(c1)) + polygon.Normal.CrossProduct(GetDirection(c2));
                    //vec = polygon.Normal.CrossProduct(GetDirection(c2));
                }
                else if (i == polygon.ListXYZPoint.Count - 1)
                {
                    Curve c1 = Line.CreateBound(polygon.ListXYZPoint[i - 1], polygon.ListXYZPoint[i]);
                    Curve c2 = Line.CreateBound(polygon.ListXYZPoint[i], polygon.ListXYZPoint[0]);
                    vec = polygon.Normal.CrossProduct(GetDirection(c1)) + polygon.Normal.CrossProduct(GetDirection(c2));
                    //vec = polygon.Normal.CrossProduct(GetDirection(c2));
                }
                else
                {
                    Curve c1 = Line.CreateBound(polygon.ListXYZPoint[i - 1], polygon.ListXYZPoint[i]);
                    Curve c2 = Line.CreateBound(polygon.ListXYZPoint[i], polygon.ListXYZPoint[i + 1]);
                    vec = polygon.Normal.CrossProduct(GetDirection(c1)) + polygon.Normal.CrossProduct(GetDirection(c2));
                    //vec = polygon.Normal.CrossProduct(GetDirection(c2));
                }
                XYZ temp;
                temp = isInside ? GeometryUtils.OffsetPoint(polygon.ListXYZPoint[i], vec, distance * Math.Sqrt(2)) : GeometryUtils.OffsetPoint(polygon.ListXYZPoint[i], -vec, distance * Math.Sqrt(2));
                //temp = (polygon.CheckXYZPointPosition(temp) == PointComparePolygonResult.Inside) ? temp : GeometryUtils.OffsetPoint(polygon.ListXYZPoint[i], -vec, distance);
                points.Add(temp);
            }
            return new Polygon(points);
        }

        /// <summary>
        /// Trả về một list<string> gồm 2 phần tử được format: {[Tên cấu kiện], [Số cấu kiện]}
        /// </summary>
        /// <param name="s">Giá trị string đang xét phải thỏa các điều kiện nhất định</param>
        /// <returns></returns>
        public static List<string> CheckString(string s)
        {
            var ss = s.Split('x', 'X', '*');
            return ss.Count() == 1 ? new List<string> { "1", ss[0] } : new List<string> { ss[0], ss[1] };
        }

        /// <summary>
        /// "Purge" một List<Curve> được lấy từ các cạnh định nghĩa một mặt trong Revit nhưng gặp một số lỗi phần mềm
        /// </summary>
        /// <param name="cs">List<Curve> đang xét</param>
        /// <returns></returns>
        public static List<Curve> Purge(List<Curve> cs)
        {
            while (true)
            {
                var check = true;
                var cs1 = new List<Curve>();
                for (var i = 0; i < cs.Count; i++)
                {
                    var l = (cs[i].GetEndPoint(0) - cs[i].GetEndPoint(1)).GetLength();
                    if (!GeometryUtils.IsBigger(l, GeometryUtils.Milimeter2Feet(10)))
                    {
                        check = false;
                        if (i == 0)
                        {
                            cs1.Add(Line.CreateBound(cs[i].GetEndPoint(0), cs[i + 1].GetEndPoint(1)));
                            i += 1;
                        }
                        else
                        {
                            cs1[cs1.Count - 1] = Line.CreateBound(cs[i - 1].GetEndPoint(0), cs[i].GetEndPoint(1));
                        }
                    }
                    else
                    {
                        cs1.Add(cs[i]);
                    }
                }

                if (check) return cs1;
                cs = cs1;
            }
        }

        /// <summary>
        /// "Purge" một List<Curve> được lấy từ các cạnh định nghĩa một mặt trong Revit nhưng gặp một số lỗi phần mềm
        /// </summary>
        /// <param name="cl">List<Curve> đang xét</param>
        /// <returns></returns>
        public static List<Curve> Purge(CurveLoop cl)
        {
            return Purge(ConvertCurveLoopToCurveList(cl));
        }
    }
}