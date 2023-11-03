using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils
{
    /// <summary>
    /// Kiểu dữ liệu Polygon định nghĩa một đa giác với các cạnh không cắt nhau nằm cùng trên một mặt phẳng
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// Trả về tập hợp đoạn thẳng là biên của Polygon
        /// </summary>
        public List<Curve> ListCurve { get; set; }

        /// <summary>
        /// Trả về tập hợp điểm là đỉnh của Polygon
        /// </summary>
        public List<XYZ> ListXYZPoint { get; private set; }

        /// <summary>
        /// Trả về tập hợp điểm 2d là đỉnh của Polygon với hệ tọa độ 2d mặt phẳng với 2 hệ trục định sẵn
        /// </summary>
        public List<UV> ListUVPoint { get; private set; }

        /// <summary>
        /// Trả về trục Ox của mặt phẳng chứa Polygon
        /// </summary>
        public XYZ XVector { get; private set; }

        /// <summary>
        /// Trả về trục Oy của mặt phẳng chứa Polygon
        /// </summary>
        public XYZ YVector { get; private set; }

        /// <summary>
        /// Trả về trục Ox tự định nghĩa trên mặt phẳng chứa Polygon
        /// </summary>
        public XYZ XVecManual { get; private set; }

        /// <summary>
        /// Trả về trục Oy tự định nghĩa trên mặt phẳng chứa Polygon
        /// </summary>
        public XYZ YVecManual { get; private set; }

        /// <summary>
        /// Trả về mặt phẳng có hệ trục tự định nghĩa chứa Polygon
        /// </summary>
        public Plane PlaneManual { get; private set; }

        /// <summary>
        /// Trả về vector pháp tuyến của mặt phẳng chứa Polygon
        /// </summary>
        public XYZ Normal { get; private set; }

        /// <summary>
        /// Trả về điểm gốc của mặt phẳng chứa Polygon
        /// </summary>
        public XYZ Origin { get; private set; }

        /// <summary>
        /// Trả về mặt ban đầu khởi tạo nên Polygon
        /// </summary>
        public PlanarFace Face { get; set; }

        /// <summary>
        /// Trả về mặt phẳng chứa Polygon
        /// </summary>
        public Plane Plane { get; private set; }

        /// <summary>
        /// Trả về điểm trọng tâm của Polygon
        /// </summary>
        public XYZ CentralXYZPoint { get; private set; }

        /// <summary>
        /// Trả về điểm 2d trọng tâm của Polygon theo hệ trục địa phương của mặt phẳng chứa Polygon
        /// </summary>
        public UV CentralUVPoint { get; private set; }

        /// <summary>
        /// Trả về 2 điểm tạo thành hình chữ nhật chứa mặt Polygon, cạnh của hình chữ nhật song song với 2 hệ trục địa phương của mặt phẳng chứa Polygon
        /// </summary>
        public List<XYZ> TwoXYZPointsBoundary { get; private set; }

        /// <summary>
        /// Trả về 2 điểm tạo thành hình vuông chứa mặt Polygon, cạnh của hình vuông song song với 2 hệ trục địa phương của mặt phẳng chứa Polygon
        /// </summary>
        public List<XYZ> TwoXYZPointsLimit { get; private set; }

        /// <summary>
        /// Trả về 2 điểm 2d tạo thành hình chữ nhật chứa Polygon, là mô phỏng của 2 điểm tạo thành hình chữ nhật chứa mặt Polygon lên hệ trục địa phương của mặt phẳng
        /// </summary>
        public List<UV> TwoUVPointsBoundary { get; private set; }

        /// <summary>
        /// Trả về 2 điểm 2d tạo thành hình vuông chứa Polygon, là mô phòng của 2 điểm tạo thành hình vuông chứa mặt Polygon lên hệ trục địa phương của mặt phẳng
        /// </summary>
        public List<UV> TwoUVPointsLimit { get; private set; }

        /// <summary>
        /// Trả về chiếu dài của hình chữ nhật chứa Polygon, song song với trục Oy của mặt phẳng chứa Polygon
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Trả về chiều rộng của hình chữ nhật chứa Polygon, song song với trục Ox của mặt phẳng chứa Polygon
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Trả về chu vi của Polygon
        /// </summary>
        public double Perimeter { get; private set; }

        /// <summary>
        /// Trả về diện tích của Polygon
        /// </summary>
        public double Area { get; private set; }

        /// <summary>
        /// Hàm khởi tạo Polygon từ mặt dạng phẳng đang xét
        /// </summary>
        /// <param name="f">Mặt dạng phẳng đang xét</param>
        public Polygon(PlanarFace f)
        {
            Face = f;
            ListCurve = CheckGeometry.GetCurves(f);
            Plane = Plane.CreateByOriginAndBasis(Face.Origin, Face.XVector, Face.YVector);

            GetParameters();
        }

        /// <summary>
        /// Hàm khởi tạo Polygon từ curveLoop là tập hợp các đoạn thẳng khép kín
        /// </summary>
        /// <param name="cl">CurveLoop đang xét</param>
        public Polygon(CurveLoop cl)
        {
            ListCurve = CheckGeometry.Purge(cl);
            var origin = ListCurve[0].GetEndPoint(0);
            var vecX = GeometryUtils.UnitVector(CheckGeometry.GetDirection(ListCurve[0]));
            var vecT = GeometryUtils.UnitVector(CheckGeometry.GetDirection(ListCurve[ListCurve.Count - 1]));
            var normal = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, vecT));
            var vecY = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, normal));
            Plane = Plane.CreateByOriginAndBasis(origin, vecX, vecY);

            GetParameters();
        }

        /// <summary>
        /// Hàm khởi tạo Polygon từ List<Curve> là tập hợp các đoạn thẳng
        /// </summary>
        /// <param name="cs">Tập hợp các đoạn thẳng đang xét thõa mãn các điều kiện nhất định</param>
        public Polygon(List<Curve> cs)
        {
            ListCurve = new List<Curve>();
            var i = 0;
            ListCurve.Add(Line.CreateBound(cs[0].GetEndPoint(0), cs[0].GetEndPoint(1)));
            while (!GeometryUtils.IsEqual(ListCurve[ListCurve.Count - 1].GetEndPoint(1), ListCurve[0].GetEndPoint(0)))
            {
                i++;
                foreach (var c in cs)
                {
                    var pnt = ListCurve[ListCurve.Count - 1].GetEndPoint(1);

                    var prePnt = ListCurve[ListCurve.Count - 1].GetEndPoint(0);

                    if (GeometryUtils.IsEqual(pnt, c.GetEndPoint(0)))
                    {
                        if (GeometryUtils.IsEqual(prePnt, c.GetEndPoint(1)))
                        {
                            continue;
                        }

                        ListCurve.Add(Line.CreateBound(c.GetEndPoint(0), c.GetEndPoint(1)));

                        break;
                    }

                    if (GeometryUtils.IsEqual(pnt, c.GetEndPoint(1)))
                    {
                        if (GeometryUtils.IsEqual(prePnt, c.GetEndPoint(0)))
                        {
                            continue;
                        }

                        ListCurve.Add(Line.CreateBound(c.GetEndPoint(1), c.GetEndPoint(0)));
                        break;
                    }
                }

                if (i == 200) throw new Exception("Error when creating polygon");
            }

            var origin = ListCurve[0].GetEndPoint(0);
            var vecX = GeometryUtils.UnitVector(CheckGeometry.GetDirection(cs[0]));
            var vecT = GeometryUtils.UnitVector(CheckGeometry.GetDirection(ListCurve[ListCurve.Count - 1]));
            var normal = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, vecT));
            var vecY = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, normal));
            Plane = Plane.CreateByOriginAndBasis(origin, vecX, vecY);

            GetParameters();
        }

        /// <summary>
        /// Hàm khởi tạo Polygon từ tập hợp các điểm đang xét
        /// </summary>
        /// <param name="points">Tập hợp các điểm phải thỏa mãn điều kiện sắp xếp theo thứ tự liên tiếp các đỉnh Polygon và nằm trong một mặt phẳng</param>
        public Polygon(IReadOnlyList<XYZ> points)
        {
            var cs = points.Select((t, i) => i < points.Count - 1 ? Line.CreateBound(t, points[i + 1]) : Line.CreateBound(t, points[0])).Cast<Curve>().ToList();

            ListCurve = cs;

            var origin = cs[0].GetEndPoint(0);
            var vecX = GeometryUtils.UnitVector(CheckGeometry.GetDirection(cs[0]));
            var vecT = GeometryUtils.UnitVector(CheckGeometry.GetDirection(ListCurve[ListCurve.Count - 1]));
            var normal = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, vecT));
            var vecY = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(vecX, normal));
            Plane = Plane.CreateByOriginAndBasis(origin, vecX, vecY);

            GetParameters();
        }

        /// <summary>
        /// Hàm gán các thuộc tính về ListXYZPoint, ListUVPoint, CentralXYZPoint, CentralUVPoint, XVector, YVector, Normal, Origin
        /// GetArea();
        /// </summary>
        private void GetParameters()
        {
            var points = (from c in ListCurve select c.GetEndPoint(0)).ToList();
            ListXYZPoint = points;

            var uvpoints = new List<UV>();
            CentralXYZPoint = new XYZ(0, 0, 0);
            foreach (var p in ListXYZPoint)
            {
                uvpoints.Add(CheckGeometry.Evaluate(this.Plane, p));
                CentralXYZPoint = GeometryUtils.AddXYZ(CentralXYZPoint, p);
            }

            ListUVPoint = uvpoints;
            CentralXYZPoint = new XYZ(CentralXYZPoint.X / ListXYZPoint.Count, CentralXYZPoint.Y / ListXYZPoint.Count, CentralXYZPoint.Z / ListXYZPoint.Count);
            CentralUVPoint = CheckGeometry.Evaluate(Plane, CentralXYZPoint);
            XVector = Plane.XVec;
            YVector = Plane.YVec;
            Normal = Plane.Normal;
            Origin = Plane.Origin;

            GetPerimeter();
            GetArea();
        }

        /// <summary>
        /// Hàm gán thống tính Area
        /// </summary>
        private void GetArea()
        {
            int j;
            double area = 0;

            for (var i = 0; i < ListUVPoint.Count; i++)
            {
                j = (i + 1) % ListUVPoint.Count;

                area += ListUVPoint[i].U * ListUVPoint[j].V;
                area -= ListUVPoint[i].V * ListUVPoint[j].U;
            }

            area /= 2;
            Area = area < 0 ? -area : area;
        }

        /// <summary>
        /// Hàm gán thuộc tính Perimeter
        /// </summary>
        private void GetPerimeter()
        {
            var len = ListCurve.Sum(c => GeometryUtils.GetLength(CheckGeometry.ConvertLine(c)));
            Perimeter = len;
        }

        /// <summary>
        /// Hàm gán thuộc tính XVecManual, YVecManual, PlaneManual
        /// </summary>
        /// <param name="vec">Vector của một trong 2 hệ trục tự định nghĩa</param>
        /// <param name="isXVector">True: vector trước là VecX, False, vector trước là VecY</param>
        public void SetManualDirection(XYZ vec, bool isXVector = true)
        {
            if (!GeometryUtils.IsEqual(GeometryUtils.DotMatrix(vec, Normal), 0)) throw new Exception("Input vector is not perpendicular with Normal!");
            XYZ xvec, yvec;
            if (isXVector)
            {
                xvec = GeometryUtils.UnitVector(vec);
                yvec = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(xvec, this.Normal));
            }
            else
            {
                yvec = GeometryUtils.UnitVector(vec);
                xvec = GeometryUtils.UnitVector(GeometryUtils.CrossMatrix(yvec, this.Normal));
            }

            XVecManual = GeometryUtils.IsBigger(xvec, -xvec) ? xvec : -xvec;
            YVecManual = GeometryUtils.IsBigger(yvec, -yvec) ? yvec : -yvec;
            PlaneManual = Plane.CreateByOriginAndBasis(CentralXYZPoint, XVecManual, YVecManual);
        }

        /// <summary>
        /// Hám gán thuộc tính TwoXYZPointsBoundary, TwoUVPointsBoundary
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="isXVector"></param>
        public void SetTwoPointsBoundary(XYZ vec, bool isXVector = true)
        {
            SetManualDirection(vec, isXVector);
            double maxU = 0, maxV = 0;
            foreach (var uvP in from xyzP in ListXYZPoint select CheckGeometry.Evaluate(this.PlaneManual, xyzP))
            {
                if (GeometryUtils.IsBigger(Math.Abs(uvP.U), maxU)) maxU = Math.Abs(uvP.U);
                if (GeometryUtils.IsBigger(Math.Abs(uvP.V), maxV)) maxV = Math.Abs(uvP.V);
            }

            var uvboundP = new UV(-maxU, -maxV);
            XYZ p1 = CheckGeometry.Evaluate(this.PlaneManual, uvboundP), p2 = CheckGeometry.Evaluate(this.PlaneManual, -uvboundP);
            TwoXYZPointsBoundary = new List<XYZ> { p1, p2 };
            TwoUVPointsBoundary = new List<UV> { CheckGeometry.Evaluate(this.Plane, p1), CheckGeometry.Evaluate(this.Plane, p2) };
        }

        /// <summary>
        /// Hàm gán thuộc tính TwoUVPointsLimit, TwoXYZPointsLimit
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="isXVector"></param>
        public void SetTwoPointsLimit(XYZ vec, bool isXVector = true)
        {
            SetManualDirection(vec, isXVector);
            double maxU = 0, maxV = 0, minU = 0, minV = 0;
            foreach (var uvP in from xyzP in ListXYZPoint select CheckGeometry.Evaluate(PlaneManual, xyzP))
            {
                if (GeometryUtils.IsBigger(uvP.U, maxU)) maxU = uvP.U;
                if (GeometryUtils.IsBigger(uvP.V, maxV)) maxV = uvP.V;
                if (GeometryUtils.IsSmaller(uvP.U, minU)) minU = uvP.U;
                if (GeometryUtils.IsSmaller(uvP.V, minV)) minV = uvP.V;
            }

            UV min = new(minU, minV), max = new(maxU, maxV);
            TwoUVPointsLimit = new List<UV> { min, max };
            XYZ p1 = CheckGeometry.Evaluate(this.PlaneManual, min), p2 = CheckGeometry.Evaluate(this.PlaneManual, max);
            TwoXYZPointsLimit = new List<XYZ> { p1, p2 };
        }

        /// <summary>
        /// Hàm gán thuộc tính Width, Height
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="isXVector"></param>
        public void SetTwoDimension(XYZ vec, bool isXVector = true)
        {
            SetManualDirection(vec, isXVector);
            double maxU = 0, maxV = 0;
            var uvPs = (from xyzP in ListXYZPoint select CheckGeometry.Evaluate(this.PlaneManual, xyzP)).ToList();
            for (var i = 0; i < uvPs.Count; i++)
            {
                for (var j = i + 1; j < uvPs.Count; j++)
                {
                    if (GeometryUtils.IsBigger(Math.Abs(uvPs[i].U - uvPs[j].U), maxU)) maxU = Math.Abs(uvPs[i].U - uvPs[j].U);
                    if (GeometryUtils.IsBigger(Math.Abs(uvPs[i].V - uvPs[j].V), maxV)) maxV = Math.Abs(uvPs[i].V - uvPs[j].V);
                }
            }

            Width = maxU;
            Height = maxV;
        }

        /// <summary>
        /// Hàm kiểm tra một điểm có nằm trong hoặc trên cạnh của Polygon hay không
        /// </summary>
        /// <param name="p">Điểm đang xét</param>
        /// <returns></returns>
        public bool IsPointInPolygon(UV p)
        {
            var polygon = ListUVPoint;
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
                if (polygon[i].V > p.V != polygon[j].V > p.V && p.U < (polygon[j].U - polygon[i].U) * (p.V - polygon[i].V) / (polygon[j].V - polygon[i].V) + polygon[i].U)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Hàm kiểm tra một điểm 2d trên mặt phẳng chứa Polygon có nằm trong hoặc trên cạnh của Polygon hay không
        /// </summary>
        /// <param name="p">Điểm 2d đang xét</param>
        /// <returns></returns>
        public bool IsPointInPolygonNewCheck(UV p)
        {
            var polygon = ListUVPoint;
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

            if (!GeometryUtils.IsBigger(p.U, minX) || !GeometryUtils.IsSmaller(p.U, maxX) || !GeometryUtils.IsBigger(p.V, minY) || !GeometryUtils.IsSmaller(p.V, maxY))
            {
                return false;
            }

            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((!GeometryUtils.IsSmaller(polygon[i].V, p.V) != (!GeometryUtils.IsSmaller(polygon[j].V, p.V)) && !GeometryUtils.IsBigger(p.U, (polygon[j].U - polygon[i].U) * (p.V - polygon[i].V) / (polygon[j].V - polygon[i].V) + polygon[i].U)))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        /// <summary>
        /// Hàm kiểm tra một đoạn thẳng có nằm trong hoặc trên cạnh của Polygon hay không
        /// </summary>
        /// <param name="l">Đoạn thẳng đang xét</param>
        /// <returns></returns>
        public bool IsLineInPolygon(Line l)
        {
            var p1 = l.GetEndPoint(0);
            if (CheckXYZPointPosition(l.GetEndPoint(0)) == PointComparePolygonResult.NonPlanar || CheckXYZPointPosition(l.GetEndPoint(0)) == PointComparePolygonResult.NonPlanar) return false;
            var len = GeometryUtils.GetLength(l);
            var dir = GeometryUtils.UnitVector(l.Direction);
            if (GeometryUtils.IsEqual(l.GetEndPoint(1), GeometryUtils.OffsetPoint(p1, dir, len)))
            {
            }
            else if (GeometryUtils.IsEqual(l.GetEndPoint(1), GeometryUtils.OffsetPoint(p1, -dir, len)))
            {
                dir = -dir;
            }
            else throw new Exception("Error when retrieve result!");

            for (var i = 0; i <= 100; i++)
            {
                var p = GeometryUtils.OffsetPoint(p1, dir, len / 100 * i);
                if (CheckXYZPointPosition(p) == PointComparePolygonResult.Outside) return false;
            }

            return true;
        }

        /// <summary>
        /// Hàm kiểm tra một đoạn thẳng có nằm ngoài hoàn toàn Polygon hay không
        /// </summary>
        /// <param name="l">Đoạn thẳng đang xét</param>
        /// <returns></returns>
        public bool IsLineOutPolygon(Line l)
        {
            var p1 = l.GetEndPoint(0);
            if (CheckXYZPointPosition(l.GetEndPoint(0)) == PointComparePolygonResult.NonPlanar || CheckXYZPointPosition(l.GetEndPoint(0)) == PointComparePolygonResult.NonPlanar) return false;
            var len = GeometryUtils.GetLength(l);
            var vec = GeometryUtils.IsEqual(GeometryUtils.AddXYZ(p1, GeometryUtils.MultiplyVector(l.Direction, len)), l.GetEndPoint(1)) ? l.Direction : -l.Direction;
            var count = 0;
            for (var i = 0; i <= 100; i++)
            {
                var p = GeometryUtils.OffsetPoint(p1, vec, len / 100 * i);
                if (CheckXYZPointPosition(p) == PointComparePolygonResult.Outside) continue;
                if (CheckXYZPointPosition(p) == PointComparePolygonResult.Inside) return false;
                count++;
            }

            return count <= 2;
        }

        /// <summary>
        /// Hàm kiểm tra vị trí tương đối của một điểm 2d trên mặt phẳng chứa Polygon so với Polygon
        /// Trả về kiểu enum PointComparePolygonResult chứa kết quả kiểm tra trên gồm các giá trị
        /// Node: Đỉnh - Boundary: Cạnh - Inside: Trong - Outside - Ngoài
        /// </summary>
        /// <param name="p">Điểm 2d đang xét</param>
        /// <returns></returns>
        public PointComparePolygonResult CheckUVPointPosition(UV p)
        {
            var polygon = ListUVPoint;
            var check1 = IsPointInPolygon(p);
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
        /// Hàm kiểm tra vị trí tương đối của một điểm so với Polygon
        /// Trả về kiểu enum PointComparePolygonResult chứa kết quả kiểm tra trên gồm các giá trị
        /// Node: Đỉnh - Boundary: Cạnh - Inside: Trong - Outside: Ngoài - NonPlanar: Ngoài mặt phẳng
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public PointComparePolygonResult CheckXYZPointPosition(XYZ p)
        {
            if (!GeometryUtils.IsEqual(CheckGeometry.GetSignedDistance(this.Plane, p), 0)) return PointComparePolygonResult.NonPlanar;
            var uvP = Evaluate(p);
            return CheckUVPointPosition(uvP);
        }

        /// <summary>
        /// Mô phỏng một điểm 3d từ một điểm 2d nằm trên mặt phẳng chứa Polygon đang xét
        /// </summary>
        /// <param name="p">Điểm 2d đang xét</param>
        /// <returns></returns>
        public XYZ Evaluate(UV p)
        {
            return CheckGeometry.Evaluate(this.Plane, p);
        }

        /// <summary>
        /// Mô phỏng một điểm 2d nằm trên mặt phẳng chứa Polygon đang xét từ một điểm 3d
        /// </summary>
        /// <param name="p">Điểm 3d đang xét</param>
        /// <returns></returns>
        public UV Evaluate(XYZ p)
        {
            return CheckGeometry.Evaluate(this.Plane, p);
        }

        /// <summary>
        /// Hàm trả về vector chỉ phương có giá trị lớn nhất theo tiêu chí so sánh Z -> Y -> X của tập hợp các đoạn thẳng định hình các cạnh Polygon
        /// </summary>
        /// <returns></returns>
        public XYZ GetTopDirectionFromCurve()
        {
            var vecs = (from c in ListCurve select GeometryUtils.UnitVector(CheckGeometry.GetDirection(c)) into vec select GeometryUtils.IsBigger(vec, -vec) ? vec : -vec).ToList();
            vecs.Sort(new ZyxComparer());
            return vecs[vecs.Count - 1];
        }

        /// <summary>
        /// Toán tử so sánh bằng 2 thể hiện kiểu dữ liệu Polygon với nhau
        /// Trả về giá trị true khi 2 Polygon chứa 2 tập hợp điểm giống nhau
        /// Khổng xét đến trình tự sắp xếp các điểm
        /// </summary>
        /// <param name="pl1">Polygon 1</param>
        /// <param name="pl2">Polygon 2</param>
        /// <returns></returns>
        public static bool operator ==(Polygon pl1, Polygon pl2)
        {
            try
            {
                List<XYZ> points = pl1.ListXYZPoint;
            }
            catch
            {
                try
                {
                    List<XYZ> points = pl2.ListXYZPoint;
                    return false;
                }
                catch
                {
                    return true;
                }
            }

            try
            {
                List<XYZ> points = pl2.ListXYZPoint;
            }
            catch
            {
                return false;
            }

            List<XYZ> pnts1 = pl1.ListXYZPoint, pnts2 = pl2.ListXYZPoint;
            pnts1.Sort(new ZyxComparer());
            pnts2.Sort(new ZyxComparer());
            return !pnts1.Where((t, i) => !GeometryUtils.IsEqual(t, pnts2[i])).Any();
        }

        /// <summary>
        /// Toán tử so sánh không bằng 2 thể hiện kiểu dữ liệu Polygon với nhau
        /// Là toán tử đối nghịch với toán tử so sánh bằng
        /// </summary>
        /// <param name="pl1">Polygon 1</param>
        /// <param name="pl2">Polygon 2</param>
        /// <returns></returns>
        public static bool operator !=(Polygon pl1, Polygon pl2)
        {
            return !(pl1 == pl2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Kiểu enum trả về kết quả so sánh khi vị trí tương đối của một điểm (3d hoặc 2d trên mặt phẳng chứa Polygon) với một Polygon
    /// Các giá trị trả về: Inside: trong, Outside: Ngoài, Boundary: Trên cạnh Polygon, Node: Đỉnh Polygon, NonPlanar: Ngoài mặt phẳng
    /// </summary>
    public enum PointComparePolygonResult
    {
        Inside,
        Outside,
        Boundary,
        Node,
        NonPlanar
    }

    /// <summary>
    /// Kiểu dữ liệu định nghĩa kết quả khi so sánh vị trí tương đối của hai đoạn thẳng
    /// </summary>
    public class LineCompareLineResult
    {
        /// <summary>
        /// Trả về kiểu enum định nghĩa kết quả so sánh vị trí tương đối của hai đoạn thẳng
        /// </summary>
        public LineCompareLineType Type { get; private set; }

        /// <summary>
        /// Trả về đoạn trùng nhau của hai đoạn thẳng
        /// Nếu 2 hai đoạn thẳng không tạo thành đoạn trùng nhau => trả về null
        /// </summary>
        public Line Line { get; private set; }

        /// <summary>
        /// Trả về tập hợp các đoạn thừa với điều kiện hai đoạn thẳng song song và có ít nhất một điểm trùng nhau
        /// Nếu 2 đoạn thẳng không thõa điều kiện trên hoặc trùng nhau hoàn toàn => trả về null
        /// </summary>
        public List<Line> ListOuterLine { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng là gộp tất cả điểm trên hai đoạn thẳng đang xét với điều kiện hai đoạn thẳng song song và có ít nhất một điểm trùng nhau
        /// Nếu 2 đoạn thẳng không thõa điều kiện trên => trả về null
        /// </summary>
        public Line MergeLine { get; private set; }

        /// <summary>
        /// Trả về điểm trùng nhau khi so sánh vị trí tương đối hai đoạn thẳng
        /// Trả về điểm bất kì nằm trong tập hợp các điểm trùng nếu hai đoạn thẳng có hơn ít nhất 2 điểm trùng nhau
        /// Trả về null khi hai đoạn thẳng hoàn toàn không trùng nhau
        /// </summary>
        public XYZ Point { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng 1 đang xét so sánh
        /// </summary>
        private Line line1;

        /// <summary>
        /// Trả về đoạn thẳng 2 đang xét so sánh
        /// </summary>
        private Line line2;

        /// <summary>
        /// Hàm khởi tạo kiểu dữ liệu định nghĩa kết quả so sánh vị trí tương đối 2 đoạn thẳng
        /// </summary>
        /// <param name="l1">Đoạn thẳng 1</param>
        /// <param name="l2">Đoạn thẳng 2</param>
        public LineCompareLineResult(Line l1, Line l2)
        {
            this.line1 = l1;
            this.line2 = l2;
            GetParameter();
        }

        /// <summary>
        /// Hàm khởi tọa kiểu dữ liệu định nghĩa kết quả so sánh vị trí tương đối 2 đoạn thẳng
        /// </summary>
        /// <param name="l1">Đoạn thẳng 1</param>
        /// <param name="l2">Đoạn thẳng 2</param>
        public LineCompareLineResult(Curve l1, Curve l2)
        {
            this.line1 = Line.CreateBound(l1.GetEndPoint(0), l1.GetEndPoint(1));
            this.line2 = Line.CreateBound(l2.GetEndPoint(0), l2.GetEndPoint(1));
            GetParameter();
        }

        /// <summary>
        /// Hàm kiểm trả vị trí tương đối của 2 đoạn thẳng
        /// Trả về các thuộc tính Line, ListOuterLine, MergeLine, Point, Type
        /// </summary>
        private void GetParameter()
        {
            XYZ vec1 = line1.Direction, vec2 = line2.Direction;
            if (GeometryUtils.IsSameOrOppositeDirection(vec1, vec2))
            {
                #region SameDirection

                if (GeometryUtils.IsEqual(CheckGeometry.GetSignedDistance(line1, line2.GetEndPoint(0)), 0))
                {
                    if (GeometryUtils.IsEqual(line1.GetEndPoint(0), line2.GetEndPoint(0)))
                    {
                        if (GeometryUtils.IsOppositeDirection(GeometryUtils.SubXYZ(line1.GetEndPoint(1), line1.GetEndPoint(0)), GeometryUtils.SubXYZ(line2.GetEndPoint(1), line2.GetEndPoint(0))))
                        {
                            Point = line1.GetEndPoint(0);
                            MergeLine = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(1));
                            Type = LineCompareLineType.SameDirectionPointOverlap;
                            return;
                        }

                        if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(1)))
                        {
                            Line = line1;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }
                        else
                        {
                            Line = line2;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }

                        ListOuterLine = new List<Line>();
                        try
                        {
                            var l = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(1));
                            ListOuterLine = new List<Line>() { l };
                        }
                        catch
                        {
                            // ignored
                        }

                        return;
                    }

                    if (GeometryUtils.IsEqual(line1.GetEndPoint(1), line2.GetEndPoint(0)))
                    {
                        if (GeometryUtils.IsOppositeDirection(GeometryUtils.SubXYZ(line1.GetEndPoint(0), line1.GetEndPoint(1)), GeometryUtils.SubXYZ(line2.GetEndPoint(1), line2.GetEndPoint(0))))
                        {
                            Point = line1.GetEndPoint(1);
                            MergeLine = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(1));
                            Type = LineCompareLineType.SameDirectionPointOverlap;
                            return;
                        }

                        if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(0)))
                        {
                            Line = line1;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }
                        else
                        {
                            Line = line2;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }

                        ListOuterLine = new List<Line>();
                        try
                        {
                            var l = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(1));
                            ListOuterLine = new List<Line>() { l };
                        }
                        catch
                        {
                            // ignored
                        }

                        return;
                    }

                    if (GeometryUtils.IsEqual(line1.GetEndPoint(1), line2.GetEndPoint(1)))
                    {
                        if (GeometryUtils.IsOppositeDirection(GeometryUtils.SubXYZ(line1.GetEndPoint(1), line1.GetEndPoint(0)), GeometryUtils.SubXYZ(line2.GetEndPoint(1), line2.GetEndPoint(0))))
                        {
                            Point = line1.GetEndPoint(1);
                            MergeLine = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(0));
                            Type = LineCompareLineType.SameDirectionPointOverlap;
                            return;
                        }

                        if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(0)))
                        {
                            Line = line1;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }
                        else
                        {
                            Line = line2;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }

                        ListOuterLine = new List<Line>();
                        try
                        {
                            var l = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(0));
                            ListOuterLine = new List<Line>() { l };
                        }
                        catch
                        {
                            // ignored
                        }

                        return;
                    }

                    if (GeometryUtils.IsEqual(line1.GetEndPoint(0), line2.GetEndPoint(1)))
                    {
                        if (GeometryUtils.IsOppositeDirection(GeometryUtils.SubXYZ(line1.GetEndPoint(0), line1.GetEndPoint(1)), GeometryUtils.SubXYZ(line2.GetEndPoint(1), line2.GetEndPoint(0))))
                        {
                            Point = line1.GetEndPoint(0);
                            MergeLine = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(0));
                            Type = LineCompareLineType.SameDirectionPointOverlap;
                            return;
                        }

                        if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(1)))
                        {
                            Line = line1;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }
                        else
                        {
                            Line = line2;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                        }

                        ListOuterLine = new List<Line>();
                        try
                        {
                            Line l = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(0));
                            ListOuterLine = new List<Line>() { l };
                        }
                        catch
                        {
                            // ignored
                        }

                        return;
                    }

                    if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(0)))
                    {
                        if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(1)))
                        {
                            Line = line1;
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                            if (CheckGeometry.IsPointInLine(Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(1)), line1.GetEndPoint(0)))
                            {
                                var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(0));
                                var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(1));
                                ListOuterLine = new List<Line> { l1, l2 };
                            }
                            else
                            {
                                var l1 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(0));
                                var l2 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(1));
                                ListOuterLine = new List<Line> { l1, l2 };
                            }

                            return;
                        }

                        if (CheckGeometry.IsPointInLine(line1, line2.GetEndPoint(0)))
                        {
                            Line = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(0));
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                            var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(1));
                            var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(0));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }
                        else
                        {
                            Line = Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(1));
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                            var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(0));
                            var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(1));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }

                        return;
                    }

                    if (CheckGeometry.IsPointInLine(line2, line1.GetEndPoint(1)))
                    {
                        if (CheckGeometry.IsPointInLine(line1, line2.GetEndPoint(0)))
                        {
                            Line = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(0));
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                            var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(0));
                            var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(1));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }
                        else
                        {
                            Line = Line.CreateBound(line1.GetEndPoint(1), line2.GetEndPoint(1));
                            Type = LineCompareLineType.SameDirectionLineOverlap;
                            var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(1));
                            var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(0));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }

                        return;
                    }

                    if (CheckGeometry.IsPointInLine(line1, line2.GetEndPoint(0)))
                    {
                        Line = line2;
                        Type = LineCompareLineType.SameDirectionLineOverlap;
                        if (CheckGeometry.IsPointInLine(Line.CreateBound(line1.GetEndPoint(0), line2.GetEndPoint(1)), line2.GetEndPoint(0)))
                        {
                            var l1 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(0));
                            var l2 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(1));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }
                        else
                        {
                            var l1 = Line.CreateBound(line2.GetEndPoint(1), line1.GetEndPoint(0));
                            var l2 = Line.CreateBound(line2.GetEndPoint(0), line1.GetEndPoint(1));
                            ListOuterLine = new List<Line> { l1, l2 };
                        }

                        return;
                    }

                    Type = LineCompareLineType.SameDirectionNonOverlap;
                    return;
                }

                Type = LineCompareLineType.Parallel;
                return;

                #endregion SameDirection
            }

            XYZ p1 = line1.GetEndPoint(0), p2 = line1.GetEndPoint(1), p3 = line2.GetEndPoint(0), p4 = line2.GetEndPoint(1);
            if (CheckGeometry.IsPointInLineOrExtend(line2, p1))
            {
                Point = p1;
                if (CheckGeometry.IsPointInLine(line2, p1))
                {
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                Type = LineCompareLineType.NonIntersectPlanar;
                return;
            }

            if (CheckGeometry.IsPointInLineOrExtend(line2, p2))
            {
                Point = p2;
                if (CheckGeometry.IsPointInLine(line2, p2))
                {
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                Type = LineCompareLineType.NonIntersectPlanar;
                return;
            }

            if (CheckGeometry.IsPointInLineOrExtend(line1, p3))
            {
                Point = p3;
                if (CheckGeometry.IsPointInLine(line1, p3))
                {
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                Type = LineCompareLineType.NonIntersectPlanar;
                return;
            }

            if (CheckGeometry.IsPointInLineOrExtend(line1, p4))
            {
                Point = p4;
                if (CheckGeometry.IsPointInLine(line1, p4))
                {
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                Type = LineCompareLineType.NonIntersectPlanar;
                return;
            }

            if (GeometryUtils.IsEqual(GeometryUtils.DotMatrix(GeometryUtils.SubXYZ(p1, p3), GeometryUtils.CrossMatrix(vec1, vec2)), 0))
            {
                double h1 = CheckGeometry.GetSignedDistance(line2, p1), h2 = CheckGeometry.GetSignedDistance(line2, p2);
                double deltaH, L1;
                var L = GeometryUtils.GetLength(p1, p2);
                XYZ pP1 = CheckGeometry.GetProjectPoint(line2, p1), pP2 = CheckGeometry.GetProjectPoint(line2, p2);
                if (GeometryUtils.IsEqual(pP1, p1))
                {
                    Point = p1;
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                if (GeometryUtils.IsEqual(pP2, p2))
                {
                    Point = p2;
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                XYZ tP1, tP2;
                if (GeometryUtils.IsSameDirection(GeometryUtils.SubXYZ(pP1, p1), GeometryUtils.SubXYZ(pP2, p2)))
                {
                    deltaH = Math.Abs(h1 - h2);
                    L1 = L * h1 / deltaH;
                    tP1 = GeometryUtils.OffsetPoint(p1, line1.Direction, L1);
                    tP2 = GeometryUtils.OffsetPoint(p1, line1.Direction, -L1);
                    if (CheckGeometry.IsPointInLineOrExtend(line2, tP1))
                    {
                        this.Point = tP1;
                    }
                    else if (CheckGeometry.IsPointInLineOrExtend(line2, tP2))
                    {
                        this.Point = tP2;
                    }
                    else
                    {
                        throw new Exception("Two points is not in line extend!");
                    }

                    Type = LineCompareLineType.NonIntersectPlanar;
                    return;
                }

                deltaH = h1 + h2;
                L1 = L * h1 / deltaH;
                tP1 = GeometryUtils.OffsetPoint(p1, line1.Direction, L1);
                tP2 = GeometryUtils.OffsetPoint(p1, line1.Direction, -L1);
                if (CheckGeometry.IsPointInLineOrExtend(line2, tP1))
                {
                    this.Point = tP1;
                }
                else if (CheckGeometry.IsPointInLineOrExtend(line2, tP2))
                {
                    this.Point = tP2;
                }
                else
                {
                    throw new Exception("Two points is not in line extend!");
                }

                if (CheckGeometry.IsPointInLine(line2, this.Point) && CheckGeometry.IsPointInLine(line1, this.Point))
                {
                    Type = LineCompareLineType.Intersect;
                    return;
                }

                Type = LineCompareLineType.NonIntersectPlanar;
                return;
            }

            Type = LineCompareLineType.NonIntersectNonPlanar;
        }
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả trả về vị trí tương đối hai đoạn thẳng có các giá trị:
    /// SameDirectionPointOverlap: cùng phương trùng một điểm, SameDirectionNonOverlap: cùng phương không giao nhau
    /// SameDirectionLineOverlap: cùng phương trùng ít nhất hai điểm, Parallel: song song
    /// Intersect: cắt nhau đồng phẳng, NonIntersectPlanar: đồng phẳng không giao nhau
    /// NonIntersectNonPlanar: không đồng phẳng không giao nhau
    /// </summary>
    public enum LineCompareLineType
    {
        SameDirectionPointOverlap,
        SameDirectionNonOverlap,
        SameDirectionLineOverlap,
        Parallel,
        Intersect,
        NonIntersectPlanar,
        NonIntersectNonPlanar
    }

    /// <summary>
    /// Kiểu dữ liệu định nghĩa kết quả trả về khi so sánh vị trí tương đối của một đoạn thẳng và một Polygon
    /// </summary>
    public class LineComparePolygonResult
    {
        /// <summary>
        /// Trả về kiểu enum định nghĩa kết quả so sánh vị trí tương đối của đoạn thẳng và Polygon
        /// </summary>
        public LineComparePolygonType Type { get; private set; }

        /// <summary>
        /// Trả về tập hợp các đoạn thẳng là phần giao giữa đoạn thằng và Polygon
        /// </summary>
        public List<Line> ListLine { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng là hình chiếu của đoạn thẳng lên mặt phẳng chứa Polygon
        /// Trong điều kiện đoạn thẳng và Polygon không đồng phẳng
        /// </summary>
        public Line ProjectLine { get; private set; }

        /// <summary>
        /// Trả về tập hợp các điểm giao giữa đoạn thằng và Polygon
        /// </summary>
        public List<XYZ> ListPoint { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng đang xét so sánh
        /// </summary>
        private Line line;

        /// <summary>
        /// Trả về Polygon đang xét so sánh
        /// </summary>
        private Polygon polygon;

        /// <summary>
        /// Hàm khởi tạo kiểu dữ liệu định nghĩa kết quả so sánh vị trí tương đối đoạn thẳng và Polygon
        /// </summary>
        /// <param name="plgon">Polygon đang xét</param>
        /// <param name="l">Đoạn thẳng đang xét</param>
        public LineComparePolygonResult(Polygon plgon, Line l)
        {
            this.line = l;
            this.polygon = plgon;
            GetParameter();
        }

        /// <summary>
        /// Hàm kiểm tra vị trí tương đối của đoạn thẳng và Polygon
        /// Trả về các thuộc tính ProjectLine, Type, ListLine, ListPoint
        /// </summary>
        private void GetParameter()
        {
            #region Planar

            if (GeometryUtils.IsEqual(GeometryUtils.DotMatrix(line.Direction, polygon.Normal), 0))
            {
                if (polygon.CheckXYZPointPosition(line.GetEndPoint(0)) == PointComparePolygonResult.NonPlanar)
                {
                    XYZ p11 = line.GetEndPoint(0), p21 = line.GetEndPoint(1);
                    var pP11 = CheckGeometry.GetProjectPoint(polygon.Plane, p11);
                    var pP21 = CheckGeometry.GetProjectPoint(polygon.Plane, p21);
                    var l11 = Line.CreateBound(pP11, pP21);
                    ProjectLine = l11;
                    Type = LineComparePolygonType.NonPlanarParallel;
                    return;
                }

                if (polygon.IsLineOutPolygon(line))
                {
                    Type = LineComparePolygonType.Outside;
                    return;
                }

                ListLine = new List<Line>();
                ListPoint = new List<XYZ>();
                if (polygon.IsLineInPolygon(line))
                {
                    ListLine.Add(line);
                    Type = LineComparePolygonType.Inside;
                    return;
                }

                foreach (var res2 in from c in polygon.ListCurve select new LineCompareLineResult(c, line))
                {
                    switch (res2.Type)
                    {
                        case LineCompareLineType.SameDirectionLineOverlap:
                            ListLine.Add(res2.Line);
                            break;
                        case LineCompareLineType.Intersect:
                            ListPoint.Add(res2.Point);
                            break;
                        case LineCompareLineType.SameDirectionPointOverlap:
                            break;
                        case LineCompareLineType.SameDirectionNonOverlap:
                            break;
                        case LineCompareLineType.Parallel:
                            break;
                        case LineCompareLineType.NonIntersectPlanar:
                            break;
                        case LineCompareLineType.NonIntersectNonPlanar:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (ListPoint.Count != 0)
                {
                    ListPoint.Sort(new ZyxComparer());
                    var points = new List<XYZ>();
                    for (var i = 0; i < ListPoint.Count; i++)
                    {
                        var check = true;
                        for (var j = i + 1; j < ListPoint.Count; j++)
                        {
                            if (!GeometryUtils.IsEqual(ListPoint[i], ListPoint[j])) continue;
                            check = false;
                            break;
                        }

                        if (check) points.Add(ListPoint[i]);
                    }

                    ListPoint = points;
                    if (ListPoint.Count == 1)
                    {
                        ListPoint.Insert(0, line.GetEndPoint(0));
                        ListPoint.Add(line.GetEndPoint(1));
                    }
                    else
                    {
                        if (GeometryUtils.IsEqual(ListPoint[0], line.GetEndPoint(0)))
                        {
                            if (GeometryUtils.IsEqual(ListPoint[ListPoint.Count - 1], line.GetEndPoint(1)))
                            {
                            }
                            else ListPoint.Add(line.GetEndPoint(1));
                        }
                        else if (GeometryUtils.IsEqual(ListPoint[0], line.GetEndPoint(1)))
                        {
                            if (GeometryUtils.IsEqual(ListPoint[ListPoint.Count - 1], line.GetEndPoint(0)))
                            {
                            }
                            else ListPoint.Add(line.GetEndPoint(0));
                        }
                        else if (GeometryUtils.IsEqual(ListPoint[ListPoint.Count - 1], line.GetEndPoint(0)))
                        {
                            ListPoint.Insert(0, line.GetEndPoint(1));
                        }
                        else if (GeometryUtils.IsEqual(ListPoint[ListPoint.Count - 1], line.GetEndPoint(1)))
                        {
                            ListPoint.Insert(0, line.GetEndPoint(0));
                        }
                        else if (GeometryUtils.IsSameDirection(GeometryUtils.SubXYZ(ListPoint[ListPoint.Count - 1], ListPoint[0]), GeometryUtils.SubXYZ(line.GetEndPoint(1), line.GetEndPoint(0))))
                        {
                            ListPoint.Insert(0, line.GetEndPoint(0));
                            ListPoint.Add(line.GetEndPoint(1));
                        }
                        else
                        {
                            ListPoint.Insert(0, line.GetEndPoint(1));
                            ListPoint.Add(line.GetEndPoint(0));
                        }
                    }

                    for (var i = 0; i < this.ListPoint.Count - 1; i++)
                    {
                        if (GeometryUtils.IsEqual(ListPoint[i], ListPoint[i + 1])) continue;
                        Line l;
                        try
                        {
                            l = Line.CreateBound(ListPoint[i], ListPoint[i + 1]);
                        }
                        catch
                        {
                            continue;
                        }

                        var check = true;
                        if (!polygon.IsLineInPolygon(l)) continue;
                        var check2 = false;
                        for (var j = 0; j < ListLine.Count; j++)
                        {
                            var res1 = new LineCompareLineResult(ListLine[j], l);
                            if (res1.Type == LineCompareLineType.SameDirectionLineOverlap) check = false;
                            if (res1.Type != LineCompareLineType.SameDirectionPointOverlap) continue;
                            ListLine[j] = res1.MergeLine;
                            check2 = true;
                            break;
                        }

                        if (check2) continue;
                        if (check)
                        {
                            ListLine.Add(l);
                        }
                    }

                    Type = LineComparePolygonType.OverlapOrIntersect;
                    return;
                }
            }

            #endregion Planar

            XYZ p1 = line.GetEndPoint(0), p2 = line.GetEndPoint(1);
            var pP1 = CheckGeometry.GetProjectPoint(polygon.Plane, p1);
            var pP2 = CheckGeometry.GetProjectPoint(polygon.Plane, p2);
            ListPoint = new List<XYZ>();
            if (GeometryUtils.IsEqual(pP1, pP2))
            {
                ListPoint.Add(pP1);
                if (CheckGeometry.IsPointInLine(line, pP1))
                {
                    if (polygon.CheckXYZPointPosition(pP1) != PointComparePolygonResult.Outside)
                    {
                        Type = LineComparePolygonType.PerpendicularIntersectFace;
                        return;
                    }

                    Type = LineComparePolygonType.PerpendicularIntersectPlane;
                    return;
                }

                Type = LineComparePolygonType.PerpendicularNonIntersect;
                return;
            }

            var l1 = Line.CreateBound(pP1, pP2);
            ProjectLine = l1;
            var res = new LineCompareLineResult(line, l1);
            if (res.Type == LineCompareLineType.Intersect)
            {
                var resP = polygon.CheckXYZPointPosition(res.Point);
                ListPoint.Add(res.Point);
                if (resP == PointComparePolygonResult.Outside)
                {
                    Type = LineComparePolygonType.NonPlanarIntersectPlane;
                    return;
                }

                Type = LineComparePolygonType.NonPlanarIntersectFace;
                return;
            }

            ListPoint.Add(res.Point);
            Type = LineComparePolygonType.NonPlanarNonIntersect;
        }
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả trả về vị trí tương đối của đoạn thẳng và Polygon, có các giá trị:
    /// NonPlanarIntersectPlane: không đồng phẳng, đoạn thẳng cắt mặt phẳng chứa Polygon mà không cắt qua Polygon
    /// NonPlanarIntersectFace: không đồng phẳng, đoạn thẳng cắt Polygon hoặc cắt qua vùng trong của Polygon
    /// NonPlanarNonIntersect: không đồng phẳng, đoạn thẳng không cắt mặt phẳng chứa Polygon
    /// NonPlanarParallel: đoạn thẳng song song với mặt phẳng chứa Polygon
    /// Outside: đồng phẳng, đoạn thẳng không cắt qua Polygon
    /// Inside: đồng phẳng, đoạn thẳng nằm hoàn hoàn bên trong Polygon
    /// OverlapOrIntersect: đồng thẳng, đoạn thằng cắt hoặc chồng lên một cạnh của Polygon
    /// PerpendicularNonIntersect: đoạn thẳng vuông góc với mặt phẳng chứa Polygon, không cắt qua mặt phẳng
    /// PerpendicularIntersectPlane: đoạn thẳng vuông góc với mặt phẳng chứa Polygon, cắt qua mặt phẳng mà không cắt qua Polygon
    /// PerpendicularIntersectFace: đoạn thẳng vuông góc với mặt phẳng chứa Polygon, cắt Polygon hoặc vùng trong của Polygon
    /// </summary>
    public enum LineComparePolygonType
    {
        NonPlanarIntersectPlane,
        NonPlanarIntersectFace,
        NonPlanarNonIntersect,
        NonPlanarParallel,
        Outside,
        Inside,
        OverlapOrIntersect,
        PerpendicularNonIntersect,
        PerpendicularIntersectPlane,
        PerpendicularIntersectFace
    }

    /// <summary>
    /// Kiểu dữ liệu định nghĩa kết quả trả về khi so sánh vị trí tương đối của hai Polygon
    /// </summary>
    public class PolygonComparePolygonResult
    {
        /// <summary>
        /// Kiểu enum định nghĩa kết quả khi so sánh vị trí tương đối của hai Polygon
        /// </summary>
        public PolygonComparePolygonPositionType PositionType { get; private set; }

        /// <summary>
        /// Kiểu enum định nghĩa kết quả khi kiểm tra phần giao nhau của hai Polygon
        /// </summary>
        public PolygonComparePolygonIntersectType IntersectType { get; private set; }

        /// <summary>
        /// Trả về tập hợp các đoạn thẳng là phần giao nhau giữa 2 Polygon
        /// </summary>
        public List<Line> ListLine { get; private set; }

        /// <summary>
        /// Trả về tập hợp các điểm là phần giao nhau giữa 2 Polygon
        /// </summary>
        public List<XYZ> ListPoint { get; private set; }

        /// <summary>
        /// Trả về kiểu dữ liệu định nghĩa tập hợp gồm một Polygon mẹ lồng các Polygon con bên trong
        /// Trả về giá trị khi một trong hai Polygon đang xét nằm hoàn toàn bên trong Polygon còn lại
        /// Các trường hợp còn lại trả về null
        /// </summary>
        public MultiPolygon OuterMultiPolygon { get; private set; }

        /// <summary>
        /// Trả về tập hợp các Polygon là phần giao nhau giữa 2 Polygon
        /// </summary>
        public List<Polygon> ListPolygon { get; private set; }

        /// <summary>
        /// Trả về Polygon 1 đang xét so sánh
        /// </summary>
        private Polygon polygon1;

        /// <summary>
        /// Trả về Polygon 2 đang xét so sánh
        /// </summary>
        private Polygon polygon2;

        /// <summary>
        /// Hàm khởi tạo kiểu dữ liệu định nghĩa kết quả so sánh vị trí tương đối của hai Polygon
        /// </summary>
        /// <param name="pl1">Polygon 1</param>
        /// <param name="pl2">Polygon 2</param>
        public PolygonComparePolygonResult(Polygon pl1, Polygon pl2)
        {
            polygon1 = pl1;
            polygon2 = pl2;
            GetPositionType();
            GetIntersectTypeAndOtherParameter();
        }

        /// <summary>
        /// Hàm kiểm tra vị trí tương đối của hai Polygon
        /// Trả về thuộc tính PositionType
        /// </summary>
        private void GetPositionType()
        {
            if (GeometryUtils.IsSameOrOppositeDirection(polygon1.Normal, polygon2.Normal))
            {
                if (polygon1.CheckXYZPointPosition(polygon2.ListXYZPoint[0]) != PointComparePolygonResult.NonPlanar)
                {
                    PositionType = PolygonComparePolygonPositionType.Planar;
                    return;
                }

                PositionType = PolygonComparePolygonPositionType.Parallel;
                return;
            }

            PositionType = PolygonComparePolygonPositionType.NonPlanar;
        }

        /// <summary>
        /// Hàm kiểm tra phần giao nhau giữa hai Polygon
        /// Trả về các thuộc tính IntersectType, ListLine, ListPoint, OuterMultiPolygon, ListPolygon
        /// </summary>
        private void GetIntersectTypeAndOtherParameter()
        {
            switch (PositionType)
            {
                case PolygonComparePolygonPositionType.Parallel:
                    IntersectType = PolygonComparePolygonIntersectType.NonIntersect;
                    return;

                #region NonPlanar

                case PolygonComparePolygonPositionType.NonPlanar:
                    bool check = false, check2 = false;
                    var points = new List<XYZ>();
                    var lines = new List<Line>();
                    foreach (var res in from c in polygon2.ListCurve select new LineComparePolygonResult(polygon1, CheckGeometry.ConvertLine(c)))
                    {
                        switch (res.Type)
                        {
                            case LineComparePolygonType.NonPlanarIntersectFace:
                            case LineComparePolygonType.PerpendicularIntersectFace:
                                {
                                    check = true;
                                    var checkP = points.All(point => !GeometryUtils.IsEqual(point, res.ListPoint[0]));

                                    if (checkP)
                                        points.Add(res.ListPoint[0]);
                                    break;
                                }
                            case LineComparePolygonType.OverlapOrIntersect:
                                check2 = true;
                                lines = res.ListLine;
                                break;
                            case LineComparePolygonType.NonPlanarIntersectPlane:
                                break;
                            case LineComparePolygonType.NonPlanarNonIntersect:
                                break;
                            case LineComparePolygonType.NonPlanarParallel:
                                break;
                            case LineComparePolygonType.Outside:
                                break;
                            case LineComparePolygonType.Inside:
                                break;
                            case LineComparePolygonType.PerpendicularNonIntersect:
                                break;
                            case LineComparePolygonType.PerpendicularIntersectPlane:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    if (check2)
                    {
                        if (points.Count >= 4)
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                if (GeometryUtils.IsEqual(points[i], points[i + 1])) continue;
                                var l = Line.CreateBound(points[i], points[i + 1]);
                                var check3 = true;
                                if (!polygon2.IsLineInPolygon(l)) continue;
                                var check4 = false;
                                for (var j = 0; j < lines.Count; j++)
                                {
                                    var res1 = new LineCompareLineResult(lines[j], l);
                                    if (res1.Type == LineCompareLineType.SameDirectionLineOverlap) check3 = false;
                                    if (res1.Type != LineCompareLineType.SameDirectionPointOverlap) continue;
                                    ListLine[j] = res1.MergeLine;
                                    check4 = true;
                                    break;
                                }

                                if (check4) continue;
                                if (check3)
                                {
                                    lines.Add(l);
                                }
                            }
                        }

                        ListLine = lines;
                        IntersectType = PolygonComparePolygonIntersectType.Boundary;
                        return;
                    }

                    if (check)
                    {
                        if (points.Count >= 2)
                        {
                            for (var i = 0; i < points.Count - 1; i++)
                            {
                                if (GeometryUtils.IsEqual(points[i], points[i + 1])) continue;
                                var l = Line.CreateBound(points[i], points[i + 1]);
                                var check3 = true;

                                if (!polygon2.IsLineInPolygon(l)) continue;
                                var check4 = false;
                                for (var j = 0; j < lines.Count; j++)
                                {
                                    var res1 = new LineCompareLineResult(lines[j], l);
                                    if (res1.Type == LineCompareLineType.SameDirectionLineOverlap) check3 = false;
                                    if (res1.Type != LineCompareLineType.SameDirectionPointOverlap) continue;
                                    lines[j] = res1.MergeLine;
                                    check4 = true;
                                    break;
                                }

                                if (check4) continue;
                                if (check3)
                                {
                                    lines.Add(l);
                                }
                            }

                            if (lines.Count != 0)
                            {
                                ListLine = lines;
                                IntersectType = PolygonComparePolygonIntersectType.Boundary;
                                return;
                            }
                        }

                        ListPoint = points;
                        IntersectType = PolygonComparePolygonIntersectType.Point;
                        return;
                    }

                    IntersectType = PolygonComparePolygonIntersectType.NonIntersect;
                    return;

                #endregion NonPlanar

                case PolygonComparePolygonPositionType.Planar:
                    check = false;
                    check2 = false;
                    List<Line> lines1 = new(), lines2 = new();
                    List<XYZ> points1 = new(), points2 = new();
                    foreach (var c1 in polygon1.ListCurve)
                    {
                        var res = new LineComparePolygonResult(polygon2, CheckGeometry.ConvertLine(c1));
                        switch (res.Type)
                        {
                            case LineComparePolygonType.OverlapOrIntersect:
                            case LineComparePolygonType.Inside:
                                {
                                    check2 = true;
                                    lines1.AddRange(res.ListLine);

                                    break;
                                }
                            case LineComparePolygonType.Outside:
                                {
                                    if (polygon2.CheckXYZPointPosition(c1.GetEndPoint(0)) == PointComparePolygonResult.Boundary || polygon2.CheckXYZPointPosition(c1.GetEndPoint(0)) == PointComparePolygonResult.Node)
                                    {
                                        points1.Add(c1.GetEndPoint(0));
                                        check = true;
                                    }

                                    if (polygon2.CheckXYZPointPosition(c1.GetEndPoint(1)) == PointComparePolygonResult.Boundary || polygon2.CheckXYZPointPosition(c1.GetEndPoint(1)) == PointComparePolygonResult.Node)
                                    {
                                        points1.Add(c1.GetEndPoint(1));
                                        check = true;
                                    }

                                    break;
                                }
                            case LineComparePolygonType.NonPlanarIntersectPlane:
                                break;
                            case LineComparePolygonType.NonPlanarIntersectFace:
                                break;
                            case LineComparePolygonType.NonPlanarNonIntersect:
                                break;
                            case LineComparePolygonType.NonPlanarParallel:
                                break;
                            case LineComparePolygonType.PerpendicularNonIntersect:
                                break;
                            case LineComparePolygonType.PerpendicularIntersectPlane:
                                break;
                            case LineComparePolygonType.PerpendicularIntersectFace:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    foreach (var c1 in polygon2.ListCurve)
                    {
                        var res = new LineComparePolygonResult(polygon1, CheckGeometry.ConvertLine(c1));
                        switch (res.Type)
                        {
                            case LineComparePolygonType.OverlapOrIntersect:
                            case LineComparePolygonType.Inside:
                                {
                                    check2 = true;
                                    lines2.AddRange(res.ListLine);

                                    break;
                                }
                            case LineComparePolygonType.Outside:
                                {
                                    if (polygon1.CheckXYZPointPosition(c1.GetEndPoint(0)) == PointComparePolygonResult.Boundary || polygon1.CheckXYZPointPosition(c1.GetEndPoint(0)) == PointComparePolygonResult.Node)
                                    {
                                        points2.Add(c1.GetEndPoint(0));
                                        check = true;
                                    }

                                    if (polygon1.CheckXYZPointPosition(c1.GetEndPoint(1)) == PointComparePolygonResult.Boundary || polygon1.CheckXYZPointPosition(c1.GetEndPoint(1)) == PointComparePolygonResult.Node)
                                    {
                                        points2.Add(c1.GetEndPoint(1));
                                        check = true;
                                    }

                                    break;
                                }
                        }
                    }

                    if (check2)
                    {
                        lines1.AddRange(lines2);

                        lines = new List<Line>();
                        for (var i = 0; i < lines1.Count; i++)
                        {
                            var check3 = true;
                            for (var j = i + 1; j < lines1.Count; j++)
                            {
                                var res = new LineCompareLineResult(lines1[i], lines1[j]);
                                if (res.Type != LineCompareLineType.SameDirectionLineOverlap) continue;
                                check3 = false;
                                break;
                            }

                            if (check3) lines.Add(lines1[i]);
                        }

                        ListLine = new List<Line>();
                        var nums = new List<int>();
                        for (var i = 0; i < lines.Count; i++)
                        {
                            for (int k = 0; k < nums.Count; k++)
                            {
                                //if (i == k) goto EndLoop;
                            }

                            var check6 = true;
                            Line temp = null;
                            for (var j = i + 1; j < lines.Count; j++)
                            {
                                var llRes = new LineCompareLineResult(lines[i], lines[j]);
                                if (llRes.Type != LineCompareLineType.SameDirectionPointOverlap) continue;
                                nums.Add(i);
                                nums.Add(j);
                                check6 = false;
                                temp = llRes.MergeLine;
                                break;
                            }

                            ListLine.Add(!check6 ? temp : lines[i]);
                            //EndLoop:
                            //a= 0;
                        }

                        var cs = ListLine.Cast<Curve>().ToList();

                        var pls = new List<Polygon>();
                        if (CheckGeometry.CreateListPolygon(cs, out pls))
                        {
                            ListPolygon = pls;
                            IntersectType = PolygonComparePolygonIntersectType.AreaOverlap;
                            return;
                        }

                        IntersectType = PolygonComparePolygonIntersectType.Boundary;
                        return;
                    }

                    if (check)
                    {
                        points1.AddRange(points2);

                        points = new List<XYZ>();
                        for (var i = 0; i < points1.Count; i++)
                        {
                            var check3 = true;
                            for (var j = i + 1; j < points1.Count; j++)
                            {
                                if (!GeometryUtils.IsEqual(points1[i], points1[j])) continue;
                                check3 = false;
                                break;
                            }

                            if (check3)
                            {
                                points.Add(points1[i]);
                            }
                        }

                        ListPoint = points;
                        IntersectType = PolygonComparePolygonIntersectType.Point;
                        return;
                    }

                    IntersectType = PolygonComparePolygonIntersectType.NonIntersect;
                    return;
            }

            throw new Exception("Code compiler should never be here.");
        }

        public void GetOuterPolygon(Polygon polygonCut, out object outerPolygonOrMulti)
        {
            if (polygonCut != polygon1 && polygonCut != polygon2)
                throw new Exception("Choose polygon be cut from first two polygons!");
            if (ListPolygon[0] == polygon1 || ListPolygon[0] == polygon2)
            {
                outerPolygonOrMulti = ListPolygon[0] == polygonCut ? null : new MultiPolygon(polygon1, polygon2);
            }
            else
            {
                var temp = ListPolygon.Aggregate(polygonCut, CheckGeometry.GetPolygonCut);

                outerPolygonOrMulti = temp;
            }
        }
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả khi so sánh vị trí tương đối của hai Polygon, có các giá trị
    /// Planar: đồng phẳng, NonPlanar: không đồng phẳng, Parallel: song song
    /// </summary>
    public enum PolygonComparePolygonPositionType
    {
        Planar,
        NonPlanar,
        Parallel
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả khi kiểm tra phần giao nhau của hai Polygon, có các giá trị
    /// AreaOverlap: trùng diện tích, Point: trùng điểm, Boundary: trùng cạnh, NonIntersect: không trùng nhau
    /// </summary>
    public enum PolygonComparePolygonIntersectType
    {
        AreaOverlap,
        Point,
        Boundary,
        NonIntersect
    }
}