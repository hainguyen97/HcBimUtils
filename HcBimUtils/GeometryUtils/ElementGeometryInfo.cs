using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils
{
    /// <summary>
    /// Kiểu dữ liệu chứa thông tin hình học của đối tượng dầm
    /// </summary>
    public class BeamGeometryInfo
    {
        /// <summary>
        /// Trả về vector định hướng phương dọc dầm
        /// </summary>
        public XYZ VecX { get; private set; }

        /// <summary>
        /// Trả về vector định hướng phương ngang dầm xét trong mặt cắt tiết diện
        /// </summary>
        public XYZ VecY { get; private set; }

        /// <summary>
        /// Trả về Vector (0,0,1) trong hệ tọa độ
        /// </summary>
        public XYZ VecZ { get; private set; }

        /// <summary>
        /// Trả về kiểu dữ liệu FamilyInstance
        /// </summary>
        public FamilyInstance Beam { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng gồm điểm đầu và điểm cuối dầm theo phương chiều dài dầm
        /// </summary>
        public Curve DrivingCurve { get; private set; }

        /// <summary>
        /// Trả về chiều rộng dầm
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Trả về chiều cao dầm
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Trả về chiều dài dầm
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt giữa dầm
        /// </summary>
        public Polygon CentralHorizontalPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt định dầm
        /// </summary>
        public Polygon TopPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt đáy dầm
        /// </summary>
        public Polygon BottomPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt giữa dầm
        /// </summary>
        public Polygon CentralVerticalLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt đầu dầm
        /// </summary>
        public Polygon FirstLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polgyon là kết quả của mặt phẳng Oxz cắt đuôi dầm
        /// </summary>
        public Polygon SecondLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt giữa dầm
        /// </summary>
        public Polygon CentralVerticalSectionPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt đầu dầm
        /// </summary>
        public Polygon FirstSectionPolygon { get; private set; }

        /// <summary>
        /// Trả về polgyon là kết quả của mặt phẳng Oyz cắt đuôi dầm
        /// </summary>
        public Polygon SecondSectionPolygon { get; private set; }

        /// <summary>
        /// Tạo một thể hiện của kiểu dữ liệu BeamGeometryInfo từ đối tượng dầm
        /// </summary>
        /// <param name="beam">Đối tượng dầm đang xét</param>
        public BeamGeometryInfo(FamilyInstance beam)
        {
            Beam = beam;
            GetParameters();
            GetPolygonsDefineFace();
        }

        private void GetParameters()
        {
            var type = Beam.Symbol;
            Width = type.LookupParameter("b").AsDouble();
            Height = type.LookupParameter("h").AsDouble();
        }

        private void GetPolygonsDefineFace()
        {
            if (Beam.Location is LocationCurve lc)
            {
                var c = lc.Curve;
                Length = c.Length;

                var ps = new List<XYZ> { c.GetEndPoint(0), c.GetEndPoint(1) };

                var zJp = Beam.LookupParameter("z Justification");
                var zOp = Beam.LookupParameter("z Offset Value");
                var yJp = Beam.LookupParameter("y Justification");
                var yOp = Beam.LookupParameter("y Offset Value");
                var vecX = ps[1] - ps[0];
                var vecY = XYZ.BasisZ.CrossProduct(vecX);

                var zJ = zJp.AsInteger();
                var zO = zOp.AsDouble();
                switch (zJ)
                {
                    case 0:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], XYZ.BasisZ, zO - Height / 2);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], XYZ.BasisZ, zO - Height / 2);
                        break;

                    case 1:
                    case 2:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], XYZ.BasisZ, zO);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], XYZ.BasisZ, zO);
                        break;

                    case 3:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], XYZ.BasisZ, zO + Height / 2);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], XYZ.BasisZ, zO + Height / 2);
                        break;
                }

                var yJ = yJp.AsInteger();
                var yO = yOp.AsDouble();
                switch (yJ)
                {
                    case 0:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], vecY, yO - Width / 2);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], vecY, yO - Width / 2);
                        break;

                    case 1:
                    case 2:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], vecY, yO);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], vecY, yO);
                        break;

                    case 3:
                        ps[0] = GeometryUtils.OffsetPoint(ps[0], vecY, yO + Width / 2);
                        ps[1] = GeometryUtils.OffsetPoint(ps[1], vecY, yO + Width / 2);
                        break;
                }

                ps.Sort(new ZyxComparer());
                var ps2 = new List<XYZ>() { ps[0], ps[1] };
                ps2.Sort(new XYComparer());
                DrivingCurve = Line.CreateBound(ps2[0], ps2[1]);

                var cenVerPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], XYZ.BasisZ, -Height / 2), GeometryUtils.OffsetPoint(ps[1], XYZ.BasisZ, -Height / 2), GeometryUtils.OffsetPoint(ps[0], XYZ.BasisZ, Height / 2), GeometryUtils.OffsetPoint(ps[1], XYZ.BasisZ, Height / 2) };
                cenVerPoints.Sort(new ZyxComparer());
                var temp = cenVerPoints[2];
                cenVerPoints[2] = cenVerPoints[3];
                cenVerPoints[3] = temp;

                var cenHozPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[0], vecY, Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, Width / 2) };
                cenHozPoints.Sort(new ZyxComparer());
                temp = cenHozPoints[2];
                cenHozPoints[2] = cenHozPoints[3];
                cenHozPoints[3] = temp;

                ps = new List<XYZ> { (cenVerPoints[0] + cenVerPoints[1]) / 2, (cenVerPoints[2] + cenVerPoints[3]) / 2 };
                var cenSecPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[0], vecY, Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, Width / 2) };
                cenSecPoints.Sort(new ZyxComparer());
                temp = cenSecPoints[2];
                cenSecPoints[2] = cenSecPoints[3];
                cenSecPoints[3] = temp;

                CentralHorizontalPolygon = new Polygon(cenHozPoints);
                CentralVerticalLengthPolygon = new Polygon(cenVerPoints);
                CentralVerticalSectionPolygon = new Polygon(cenSecPoints);

                vecX = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecX, -vecX) ? vecX : -vecX);
                vecY = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecY, -vecY) ? vecY : -vecY);
                TopPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, XYZ.BasisZ, Height / 2);
                BottomPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, XYZ.BasisZ, -Height / 2);
                FirstLengthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalLengthPolygon, vecY, -Width / 2);
                SecondLengthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalLengthPolygon, vecY, Width / 2);
                FirstSectionPolygon = GeometryUtils.OffsetPolygon(CentralVerticalSectionPolygon, vecX, -Length / 2);
                SecondSectionPolygon = GeometryUtils.OffsetPolygon(CentralVerticalSectionPolygon, vecX, Length / 2);

                VecX = vecX;
                VecY = vecY;
            }

            VecZ = XYZ.BasisZ;
        }

        private void CreateModelPolygon()
        {
            var doc = Beam.Document;
            CheckGeometry.CreateModelLinePolygon(doc, CentralHorizontalPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalSectionPolygon);

            CheckGeometry.CreateModelLinePolygon(doc, TopPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, BottomPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstSectionPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondSectionPolygon);
        }

        private void GetBoundingBox()
        {
            var bb = Beam.get_BoundingBox(null);
            CheckGeometry.CreateModelLine(Beam.Document, null, Line.CreateBound(bb.Max, bb.Min));
        }
    }

    /// <summary>
    /// Kiểu dữ liệu chứa thông tin hình học của đối tượng cột
    /// </summary>
    public class ColumnGeometryInfo
    {
        /// <summary>
        /// Trả về vector định hướng phương ngang dầm xét trong mặt cắt tiết diện, trùng với phương chứa chiều rộng cột định nghĩa trong Revit
        /// </summary>
        public XYZ VecX { get; private set; }

        /// <summary>
        /// Trả về vector định hướng phương ngang dầm xét trong mặt cắt tiết diện, trùng với phương chứa chiều dài cột định nghĩa trong Revit
        /// </summary>
        public XYZ VecY { get; private set; }

        /// <summary>
        /// Trả về Vector (0,0,1) trong hệ tọa độ
        /// </summary>
        public XYZ VecZ { get; private set; }

        /// <summary>
        /// Trả về kiểu dữ liệu FamilyInstance
        /// </summary>
        public FamilyInstance Column { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng gồm điểm đầu và điểm cuối cột theo phương chiều cao cột
        /// </summary>
        public Curve DrivingCurve { get; private set; }

        /// <summary>
        /// Trả về chiều rộng cột được lấy từ tham biến b định nghĩa trong Revit
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Trả về chiều rộng cột được lấy từ tham biến h định nghĩa trong Revit
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Trả về chiều cao cột
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt giữa cột
        /// </summary>
        public Polygon CentralHorizontalPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt đỉnh cột
        /// </summary>
        public Polygon TopPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt đáy cột
        /// </summary>
        public Polygon BottomPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt giữa cột
        /// </summary>
        public Polygon CentralVerticalHeightPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt đầu cột
        /// </summary>
        public Polygon FirstHeightPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt đuôi cột
        /// </summary>
        public Polygon SecondHeightPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt giữa cột
        /// </summary>
        public Polygon CentralVerticalWidthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt đầu cột
        /// </summary>
        public Polygon FirstWidthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt đuôi cột
        /// </summary>
        public Polygon SecondWidthPolygon { get; private set; }

        /// <summary>
        /// Tạo một thể hiện của kiểu dữ liệu ColumnGeometryInfo từ đối tượng cột
        /// </summary>
        /// <param name="column">Đối tượng cột đang xét</param>
        public ColumnGeometryInfo(FamilyInstance column)
        {
            this.Column = column;
            GetParameters();
            GetPolygonsDefineFace();
        }

        private void GetParameters()
        {
            var doc = Column.Document;
            var type = Column.Symbol;
            Width = type.LookupParameter("b").AsDouble();
            Height = type.LookupParameter("h").AsDouble();

            var lp = Column.Location as LocationPoint;
            var p = lp.Point;

            var blP = Column.LookupParameter("Base Level");
            var boP = Column.LookupParameter("Base Offset");

            var bl = doc.GetElement(blP.AsElementId()) as Level;
            var bo = boP.AsDouble();

            var tlP = Column.LookupParameter("Top Level");
            var toP = Column.LookupParameter("Top Offset");

            var tl = doc.GetElement(tlP.AsElementId()) as Level;
            var to = toP.AsDouble();

            DrivingCurve = Line.CreateBound(new XYZ(p.X, p.Y, bl.Elevation + bo), new XYZ(p.X, p.Y, tl.Elevation + to));

            Length = DrivingCurve.Length;
        }

        private void GetPolygonsDefineFace()
        {
            var c = DrivingCurve;

            var ps = new List<XYZ> { c.GetEndPoint(0), c.GetEndPoint(1) };
            var tf = Column.GetTransform();

            var vecX = tf.BasisX;
            var vecY = tf.BasisY;
            var vecZ = tf.BasisZ;

            var cenWidthPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecX, -Width / 2), GeometryUtils.OffsetPoint(ps[1], vecX, -Width / 2), GeometryUtils.OffsetPoint(ps[0], vecX, Width / 2), GeometryUtils.OffsetPoint(ps[1], vecX, Width / 2) };
            cenWidthPoints.Sort(new ZyxComparer());
            var temp = cenWidthPoints[2];
            cenWidthPoints[2] = cenWidthPoints[3];
            cenWidthPoints[3] = temp;

            var cenHeightPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecY, -Height / 2), GeometryUtils.OffsetPoint(ps[1], vecY, -Height / 2), GeometryUtils.OffsetPoint(ps[0], vecY, Height / 2), GeometryUtils.OffsetPoint(ps[1], vecY, Height / 2) };

            cenHeightPoints.Sort(new ZyxComparer());
            temp = cenHeightPoints[2];
            cenHeightPoints[2] = cenHeightPoints[3];
            cenHeightPoints[3] = temp;

            ps = new List<XYZ> { (cenHeightPoints[0] + cenHeightPoints[3]) / 2, (cenHeightPoints[1] + cenHeightPoints[2]) / 2 };
            var cenHozPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecX, -Width / 2), GeometryUtils.OffsetPoint(ps[1], vecX, -Width / 2), GeometryUtils.OffsetPoint(ps[0], vecX, Width / 2), GeometryUtils.OffsetPoint(ps[1], vecX, Width / 2) };
            cenHozPoints.Sort(new ZyxComparer());
            temp = cenHozPoints[2];
            cenHozPoints[2] = cenHozPoints[3];
            cenHozPoints[3] = temp;

            CentralVerticalHeightPolygon = new Polygon(cenHeightPoints);
            CentralVerticalWidthPolygon = new Polygon(cenWidthPoints);
            CentralHorizontalPolygon = new Polygon(cenHozPoints);

            vecX = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecX, -vecX) ? vecX : -vecX);
            vecY = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecY, -vecY) ? vecY : -vecY);
            vecZ = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecZ, -vecZ) ? vecZ : -vecZ);
            TopPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, vecZ, Length / 2);
            BottomPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, vecZ, -Length / 2);
            FirstHeightPolygon = GeometryUtils.OffsetPolygon(CentralVerticalHeightPolygon, vecX, -Width / 2);
            SecondHeightPolygon = GeometryUtils.OffsetPolygon(CentralVerticalHeightPolygon, vecX, Width / 2);
            FirstWidthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalWidthPolygon, vecY, -Height / 2);
            SecondWidthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalWidthPolygon, vecY, Height / 2);

            VecX = vecX;
            VecY = vecY;
            VecZ = vecZ;
        }

        private void CreateModelPolygon()
        {
            var doc = Column.Document;
            CheckGeometry.CreateModelLinePolygon(doc, CentralHorizontalPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalHeightPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalWidthPolygon);

            CheckGeometry.CreateModelLinePolygon(doc, TopPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, BottomPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstHeightPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondHeightPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstWidthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondWidthPolygon);
        }
    }

    /// <summary>
    /// Kiểu dữ liệu chứa thông tin hình học của đối tượng vách
    /// </summary>
    public class WallGeometryInfo
    {
        /// <summary>
        /// Trả về vector định hướng phương ngang dầm xét trong mặt cắt tiết diện, trùng với phương chứa chiều rộng vách định nghĩa trong Revit
        /// </summary>
        public XYZ VecX { get; private set; }

        /// <summary>
        /// Trả về vector định hướng phương ngang dầm xét trong mặt cắt tiết diện, trùng với phương chứa chiều dài vách định nghĩa trong Revit
        /// </summary>
        public XYZ VecY { get; private set; }

        /// <summary>
        /// Trả về Vector (0,0,1) trong hệ tọa độ
        /// </summary>
        public XYZ VecZ { get; private set; }

        /// <summary>
        /// Trả về kiểu dữ liệu Wall
        /// </summary>
        public Wall Wall { get; private set; }

        /// <summary>
        /// Trả về đoạn thẳng gồm điểm đầu và điểm cuối cột theo phương chiều dài vách
        /// </summary>
        public Curve DrivingCurve { get; private set; }

        /// <summary>
        /// Trả về chiều rộng vách được lấy từ tham biến Width định nghĩa trong Revit
        /// </summary>
        public double Width { get; private set; }

        /// <summary>
        /// Trả về chiều cao vách
        /// </summary>
        public double Height { get; private set; }

        /// <summary>
        /// Trả về chiều dài vách được định nghĩa trong Revit
        /// </summary>
        public double Length { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt giữa vách
        /// </summary>
        public Polygon CentralHorizontalPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt đỉnh vách
        /// </summary>
        public Polygon TopPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxy cắt đáy vách
        /// </summary>
        public Polygon BottomPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt giữa vách
        /// </summary>
        public Polygon CentralVerticalLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt đầu vách
        /// </summary>
        public Polygon FirstLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oyz cắt đuôi vách
        /// </summary>
        public Polygon SecondLengthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt giữa vách
        /// </summary>
        public Polygon CentralVerticalWidthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt đầu vách
        /// </summary>
        public Polygon FirstWidthPolygon { get; private set; }

        /// <summary>
        /// Trả về polygon là kết quả của mặt phẳng Oxz cắt đuôi vách
        /// </summary>
        public Polygon SecondWidthPolygon { get; private set; }

        /// <summary>
        /// Tạo một thể hiện của kiểu dữ liệu WallGeometryInfo từ đối tượng vách
        /// </summary>
        /// <param name="beam">Đối tượng vách đang xét</param>
        public WallGeometryInfo(Wall wall)
        {
            this.Wall = wall;
            GetParameters();
            GetPolygonsDefineFace();
        }

        private void GetParameters()
        {
            var type = Wall.WallType;
            Width = type.LookupParameter("Width").AsDouble();

            var lc = Wall.Location as LocationCurve;
            var c = lc.Curve;

            var boP = Wall.LookupParameter("Base Offset");
            c = GeometryUtils.OffsetCurve(c, XYZ.BasisZ, boP.AsDouble());
            var ps = new List<XYZ> { c.GetEndPoint(0), c.GetEndPoint(1) };
            ps.Sort(new ZyxComparer());

            DrivingCurve = Line.CreateBound(ps[0], ps[1]);
            Length = DrivingCurve.Length;

            var uhP = Wall.LookupParameter("Unconnected Height");
            Height = uhP.AsDouble();
        }

        private void GetPolygonsDefineFace()
        {
            var c = DrivingCurve;

            var vecX = CheckGeometry.GetDirection(c);
            var vecZ = XYZ.BasisZ;
            var vecY = vecZ.CrossProduct(vecX);

            var ps = new List<XYZ> { c.GetEndPoint(0), c.GetEndPoint(1) };

            var cenLengthPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecZ, 0), GeometryUtils.OffsetPoint(ps[1], vecZ, 0), GeometryUtils.OffsetPoint(ps[0], vecZ, Height), GeometryUtils.OffsetPoint(ps[1], vecZ, Height) };
            cenLengthPoints.Sort(new ZyxComparer());
            var temp = cenLengthPoints[2];
            cenLengthPoints[2] = cenLengthPoints[3];
            cenLengthPoints[3] = temp;

            var midPoint = (ps[0] + ps[1]) / 2;
            ps = new List<XYZ> { GeometryUtils.OffsetPoint(midPoint, vecY, -Width / 2), GeometryUtils.OffsetPoint(midPoint, vecY, Width / 2) };
            var cenWidthPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecZ, 0), GeometryUtils.OffsetPoint(ps[1], vecZ, 0), GeometryUtils.OffsetPoint(ps[0], vecZ, Height), GeometryUtils.OffsetPoint(ps[1], vecZ, Height) };
            cenWidthPoints.Sort(new ZyxComparer());
            temp = cenWidthPoints[2];
            cenWidthPoints[2] = cenWidthPoints[3];
            cenWidthPoints[3] = temp;

            ps = new List<XYZ> { (cenLengthPoints[0] + cenLengthPoints[3]) / 2, (cenLengthPoints[1] + cenLengthPoints[2]) / 2 };
            var cenHozPoints = new List<XYZ> { GeometryUtils.OffsetPoint(ps[0], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, -Width / 2), GeometryUtils.OffsetPoint(ps[0], vecY, Width / 2), GeometryUtils.OffsetPoint(ps[1], vecY, Width / 2) };
            cenHozPoints.Sort(new ZyxComparer());
            temp = cenHozPoints[2];
            cenHozPoints[2] = cenHozPoints[3];
            cenHozPoints[3] = temp;

            CentralVerticalLengthPolygon = new Polygon(cenLengthPoints);
            CentralVerticalWidthPolygon = new Polygon(cenWidthPoints);
            CentralHorizontalPolygon = new Polygon(cenHozPoints);

            vecX = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecX, -vecX) ? vecX : -vecX);
            vecY = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecY, -vecY) ? vecY : -vecY);
            vecZ = GeometryUtils.UnitVector(GeometryUtils.IsBigger(vecZ, -vecZ) ? vecZ : -vecZ);

            TopPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, vecZ, Height / 2);
            BottomPolygon = GeometryUtils.OffsetPolygon(CentralHorizontalPolygon, vecZ, -Height / 2);
            FirstLengthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalLengthPolygon, vecY, -Width / 2);
            SecondLengthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalLengthPolygon, vecY, Width / 2);
            FirstWidthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalWidthPolygon, vecX, -Length / 2);
            SecondWidthPolygon = GeometryUtils.OffsetPolygon(CentralVerticalWidthPolygon, vecX, Length / 2);

            VecX = vecX;
            VecY = vecY;
            VecZ = vecZ;
        }

        private void CreateModelPolygon()
        {
            var doc = Wall.Document;
            CheckGeometry.CreateModelLinePolygon(doc, CentralHorizontalPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, CentralVerticalWidthPolygon);

            CheckGeometry.CreateModelLinePolygon(doc, TopPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, BottomPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondLengthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, FirstWidthPolygon);
            CheckGeometry.CreateModelLinePolygon(doc, SecondWidthPolygon);
        }
    }
}