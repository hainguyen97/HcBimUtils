#region Namespaces

using Autodesk.Revit.DB;

#endregion Namespaces

namespace HcBimUtils.GeometryUtils
{
    /// <summary>
    /// Kiểu dữ liệu định nghĩa tập hợp gồm một Polygon mẹ lồng các Polygon con bên trong
    /// Các Polygon con phải nằm hoàn toàn bên trong Polygon mẹ
    /// </summary>
    public class MultiPolygon
    {
        /// <summary>
        /// Trả về tập hợp các điểm là đỉnh của Polygon mẹ
        /// </summary>
        public List<XYZ> ListXYZPoint { get; private set; }

        /// <summary>
        /// Trả về tập hợp gồm 2 điểm tạo thành hình chữ nhật chứa MutiPolygon
        /// Cạnh của hình chữ nhật song song với 2 hệ trục địa phương của mặt phẳng chứa MultiPolygon
        /// </summary>
        public List<XYZ> TwoXYZPointsBoundary { get; private set; }

        /// <summary>
        /// Trả về tập hợp gồm 2 điểm tạo thành hình chữ nhật chứa MutiPolygon
        /// Là mô phỏng của 2 điểm tạo thành hình chữ nhật chứa MutliPolygon lên hệ trục địa phương của mặt phẳng chứa MultiPolygon
        /// </summary>
        public List<UV> TwoUVPointsBoundary { get; private set; }

        /// <summary>
        /// Trả về điểm trọng tâm của Polygon mẹ
        /// </summary>
        public XYZ CentralXYZPoint { get; private set; }

        /// <summary>
        /// Trả về trục Ox tự định nghĩa trên mặt phẳng chứa MultiPolygon
        /// </summary>
        public XYZ XVecManual { get; private set; }

        /// <summary>
        /// Trả về trục Oy tự định nghĩa trên mặt phẳng chứa MultiPolygon
        /// </summary>
        public XYZ YVecManual { get; private set; }

        /// <summary>
        /// Trả về vector pháp tuyến của mặt phẳng chứa MultiPolygon
        /// </summary>
        public XYZ Normal { get; private set; }

        /// <summary>
        /// Trả về mặt phẳng chứa MultiPolygon
        /// </summary>
        public Plane Plane { get; private set; }

        /// <summary>
        /// Trả về mặt phẳng có hệ trục tự định nghĩa chứa MultiPolygon
        /// </summary>
        public Plane PlaneManual { get; private set; }

        /// <summary>
        /// Trả về Polygon mẹ
        /// </summary>
        public Polygon SurfacePolygon { get; private set; }

        /// <summary>
        /// Trả về tập hợp các Polygon con
        /// Trả về null khi không có Polygon con nào
        /// </summary>
        public List<Polygon> OpeningPolygons { get; private set; }

        /// <summary>
        /// Hàm khởi tạo MultiPolygon từ mặt đang xét
        /// </summary>
        /// <param name="f">Mặt đang xét</param>
        public MultiPolygon(Face f)
        {
            var pls = new List<Polygon>();
            var eAA = f.EdgeLoops;
            foreach (EdgeArray eA in eAA)
            {
                var cs = (from Edge e in eA select e.Tessellate() as List<XYZ> into points select Line.CreateBound(points[0], points[1])).Cast<Curve>().ToList();

                pls.Add(new Polygon(cs));
                if (eAA.Size != 1) continue;
                SurfacePolygon = pls[0];
                OpeningPolygons = new List<Polygon>();
                goto GetParameters;
            }

            for (var i = 0; i < pls.Count; i++)
            {
                var plane = pls[i].Plane;
                for (var j = i + 1; j < pls.Count; j++)
                {
                    var tempPoly = CheckGeometry.GetProjectPolygon(plane, pls[j]);
                    var res = new PolygonComparePolygonResult(pls[i], tempPoly);
                    if (res.IntersectType != PolygonComparePolygonIntersectType.AreaOverlap) continue;
                    if (res.ListPolygon[0] == pls[i])
                    {
                        SurfacePolygon = pls[j];
                        goto FinishLoops;
                    }

                    if (res.ListPolygon[0] == pls[j])
                    {
                        SurfacePolygon = pls[i];
                        goto FinishLoops;
                    }

                    throw new Exception("Face must contain polygons inside polygon!");
                }
            }

        FinishLoops:
            if (SurfacePolygon == null) throw new Exception("Error when retrieve surface polygon!");
            Plane = SurfacePolygon.Plane;
            OpeningPolygons = new List<Polygon>();
            foreach (var tempPoly in from pl in pls where pl != SurfacePolygon select CheckGeometry.GetProjectPolygon(Plane, pl))
            {
                OpeningPolygons.Add(tempPoly);
            }

        GetParameters:
            GetParameters();
        }

        /// <summary>
        /// Hàm khởi tạo MultiPolygon từ Polygon mẹ và các Polygon con
        /// Các Polygon phải đồng phẳng và các Polygon con phải nằm hoàn toàn bên trong Polygon mẹ
        /// </summary>
        /// <param name="surPolygon">Polygon mẹ đang xét</param>
        /// <param name="openPolygons">Tập hợp các Polygon con đang xét</param>
        public MultiPolygon(Polygon surPolygon, List<Polygon> openPolygons)
        {
            SurfacePolygon = surPolygon;
            OpeningPolygons = openPolygons;

            GetParameters();
        }

        /// <summary>
        /// Hàm khởi tạo MultiPolygon từ Polygon mẹ và một Polygon con
        /// Các Polygon phải đồng phẳng và Polygon con phải nằm hoàn toàn bên trong Polygon mẹ
        /// </summary>
        /// <param name="surPolygon">Polygon mẹ đang xét</param>
        /// <param name="openPolygon">Polygon con đang xét</param>
        public MultiPolygon(Polygon surPolygon, Polygon openPolygon)
        {
            SurfacePolygon = surPolygon;
            OpeningPolygons = new List<Polygon> { openPolygon };

            GetParameters();
        }

        /// <summary>
        /// Hàm gán các thuộc tính ListXYZPoint, Normal, CentralXYZPoint
        /// </summary>
        private void GetParameters()
        {
            ListXYZPoint = SurfacePolygon.ListXYZPoint;
            Normal = SurfacePolygon.Normal;
            CentralXYZPoint = SurfacePolygon.CentralXYZPoint;
        }

        /// <summary>
        /// Hàm gán các thuộc tính XVecManual, YVecManual, PlaneManual
        /// </summary>
        /// <param name="vec">Vector của một trong 2 hệ trục tự định nghĩa</param>
        /// <param name="isXVector">True: vector trước là VecX, False, vector trước là VecY</param>
        public void SetManualDirection(XYZ vec, bool isXVector = true)
        {
            SurfacePolygon.SetManualDirection(vec, isXVector);
            XVecManual = SurfacePolygon.XVecManual;
            YVecManual = SurfacePolygon.YVecManual;
            PlaneManual = SurfacePolygon.PlaneManual;
        }

        /// <summary>
        /// Hàm gán các thuộc tính TwoUVPointsBoundary, TwoXYZPointsBoundary
        /// </summary>
        /// <param name="vec">Vector của một trong 2 hệ trục tự định nghĩa</param>
        /// <param name="isXVector">True: vector trước là VecX, False, vector trước là VecY</param>
        public void SetTwoPointsBoundary(XYZ vec, bool isXVector = true)
        {
            SetManualDirection(vec, isXVector);
            SurfacePolygon.SetTwoPointsBoundary(vec, isXVector);
            TwoUVPointsBoundary = SurfacePolygon.TwoUVPointsBoundary;
            TwoXYZPointsBoundary = SurfacePolygon.TwoXYZPointsBoundary;
        }
    }

    /// <summary>
    /// Kiểu dữ liệu định nghĩa kết quả trả về khi so sánh vị trí tương đối của một Polygon và một MultiPolygon
    /// </summary>
    public class PolygonCompareMultiPolygonResult
    {
        /// <summary>
        /// Kiểu enum định nghĩa kết quả khi so sánh vị trí tương đối của một Polygon và một MultiPolygon
        /// </summary>
        public PolygonCompareMultiPolygonPositionType PositionType { get; private set; }

        /// <summary>
        /// Kiểu enum định nghĩa kết quả khi kiểm tra phần giao nhau của một Polygon và một MultiPolygon
        /// </summary>
        public PolygonCompareMultiPolygonIntersectType IntersectType { get; private set; }

        /// <summary>
        /// Trả về tập hợp các đoạn thẳng là phần giao giữa nhau một Polygon và một MultiPolygon
        /// </summary>
        public List<Line> ListLine { get; private set; }

        /// <summary>
        /// Trả về tập hợp các điểm là phần giao nhau giữa nhau một Polygon và một MultiPolygon
        /// </summary>
        public List<XYZ> ListPoint { get; private set; }

        /// <summary>
        /// Trả về tập hợp các Polygon là phần giao nhau giữa một Polygon và một MultiPolygon
        /// </summary>
        public List<Polygon> ListPolygon { get; private set; }

        /// <summary>
        /// Trả về Polygon đang xét so sánh
        /// </summary>
        private Polygon polygon;

        /// <summary>
        /// Trả về MultiPolygon đang xét so sánh
        /// </summary>
        private MultiPolygon multiPolygon;

        /// <summary>
        /// Trả về MultiPolygon là phần giao giữa một Polygon và một MultiPolygon
        /// </summary>
        public MultiPolygon MultiPolygon;

        /// <summary>
        /// Hàm khởi tạo kiểu dữ liệu định nghĩa kết quả so sánh vị trí tương đối của một Polygon và một MultiPolygon
        /// </summary>
        /// <param name="pl">Polygon đang xét</param>
        /// <param name="mpl">MultiPolygon đang xét</param>
        public PolygonCompareMultiPolygonResult(Polygon pl, MultiPolygon mpl)
        {
            polygon = pl;
            multiPolygon = mpl;
            GetPositionType();
            GetIntersectTypeAndOtherParameter();
        }

        /// <summary>
        /// Hàm kiểm tra vị trí tương đối của Polygon và MultiPolygon, trả về thuộc tính PositionType
        /// </summary>
        private void GetPositionType()
        {
            if (GeometryUtils.IsSameOrOppositeDirection(polygon.Normal, multiPolygon.Normal))
            {
                if (multiPolygon.SurfacePolygon.CheckXYZPointPosition(polygon.ListXYZPoint[0]) != PointComparePolygonResult.NonPlanar)
                {
                    PositionType = PolygonCompareMultiPolygonPositionType.Planar;
                    return;
                }

                PositionType = PolygonCompareMultiPolygonPositionType.Parallel;
                return;
            }

            PositionType = PolygonCompareMultiPolygonPositionType.NonPlarnar;
        }

        /// <summary>
        /// Hàm kiểm tra phần giao nhau giữa Polygon và MultiPolygon
        /// Trả về các thuộc tính IntersectType, ListLine, ListPoint, ListPolygon, MultiPolygon
        /// </summary>
        private void GetIntersectTypeAndOtherParameter()
        {
            switch (PositionType)
            {
                case PolygonCompareMultiPolygonPositionType.Parallel:
                    IntersectType = PolygonCompareMultiPolygonIntersectType.NonIntersect;
                    return;
                case PolygonCompareMultiPolygonPositionType.NonPlarnar: throw new Exception("Code for this case hasn't finished yet!");
                case PolygonCompareMultiPolygonPositionType.Planar:
                    var surPL = multiPolygon.SurfacePolygon;
                    var res = new PolygonComparePolygonResult(polygon, surPL);
                    switch (res.IntersectType)
                    {
                        case PolygonComparePolygonIntersectType.NonIntersect:
                            IntersectType = PolygonCompareMultiPolygonIntersectType.NonIntersect;
                            return;
                        case PolygonComparePolygonIntersectType.Boundary:
                            IntersectType = PolygonCompareMultiPolygonIntersectType.Boundary;
                            ListLine = res.ListLine;
                            return;
                        case PolygonComparePolygonIntersectType.Point:
                            IntersectType = PolygonCompareMultiPolygonIntersectType.Point;
                            ListPoint = res.ListPoint;
                            return;
                        case PolygonComparePolygonIntersectType.AreaOverlap:
                            ListPolygon = new List<Polygon>();
                            MultiPolygon = null;
                            foreach (var pl in res.ListPolygon)
                            {
                                var temp = pl;
                                foreach (var openPl in multiPolygon.OpeningPolygons)
                                {
                                    var ppRes = new PolygonComparePolygonResult(temp, openPl);
                                    if (ppRes.IntersectType != PolygonComparePolygonIntersectType.AreaOverlap) continue;
                                    object polyorMultiPolygonCut;
                                    ppRes.GetOuterPolygon(temp, out polyorMultiPolygonCut);
                                    switch (polyorMultiPolygonCut)
                                    {
                                        case null:
                                            goto Here;
                                        case MultiPolygon cut:
                                            MultiPolygon = cut;
                                            return;
                                    }

                                    temp = polyorMultiPolygonCut as Polygon;
                                }

                                ListPolygon.Add(temp);
                            Here:;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả khi so sánh vị trí tương đối của một Polygon và một MultiPolygon, có các giá trị:
    /// Planar: đồng phẳng, NonPlanar: không đồng phẳng, Parallel: song song
    /// </summary>
    public enum PolygonCompareMultiPolygonPositionType
    {
        Planar,
        NonPlarnar,
        Parallel
    }

    /// <summary>
    /// Kiểu enum định nghĩa kết quả khi kiểm tra phần giao nhau của một Polygon và một MultiPolygon, có các giá trị:
    /// AreaOverlap: trùng diện tích, Point: trùng điểm, Boundary: trùng cạnh, NonIntersect: không trùng nhau
    /// </summary>
    public enum PolygonCompareMultiPolygonIntersectType
    {
        AreaOverlap,
        Point,
        Boundary,
        NonIntersect
    }
}