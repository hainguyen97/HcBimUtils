using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using HcBimUtils.DocumentUtils;

namespace HcBimUtils
{
    public static class SolidUtil
    {
        public static PlanarFace TopFace(this Solid solid)
        {
            PlanarFace topFace = solid.Faces
               .Cast<Face>()
               .Where(x => x.ComputeNormal(UV.Zero).IsCodirectionalTo(XYZ.BasisZ))
               .OrderByDescending(x => x.Evaluate(UV.Zero).Z).FirstOrDefault() as PlanarFace;
            return topFace;
        }
        public static PlanarFace BottomFace(this Solid solid)
        {
            PlanarFace bottomFace = solid.Faces
               .Cast<Face>()
               .Where(x => x.ComputeNormal(UV.Zero).IsCodirectionalTo(XYZ.BasisZ.Negate()))
               .OrderBy(x => x.Evaluate(UV.Zero).Z).FirstOrDefault() as PlanarFace;
            return bottomFace;
        }
        public static List<Solid> GetSymbolSolids(this FamilyInstance fa)
        {
            List<Solid> solids = new List<Solid>();
#if R24
            return solids;
#else
    
            if (fa != null)
            {
                Options opt = new Options()
                {
                    ComputeReferences = true,
                    DetailLevel = ViewDetailLevel.Fine,
                    IncludeNonVisibleObjects = true
                };
                Transform trf = fa.GetTransform();
                foreach (GeometryObject geoObj in fa.get_Geometry(opt))
                {
                    if (geoObj is GeometryInstance)
                    {
                        GeometryInstance geoIns = geoObj as GeometryInstance;
                        FamilySymbol symbol = geoIns.Symbol as FamilySymbol;
                        foreach (GeometryObject geoObj2 in symbol.get_Geometry(opt).GetTransformed(trf))
                        {
                            if (geoObj2 is Solid)
                            {
                                Solid solid = geoObj2 as Solid;
                                if (solid.Volume > 0)
                                {
                                    solids.Add(solid);
                                }
                            }
                        }
                    }
                }
            }

            return solids;
#endif
        }
        public static List<Solid> GetSolids(Element element, Transform transform = null)
        {
            Document doc = element.Document;
            List<Solid> solids = new List<Solid>();
            bool check = false;
            Options options = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = true
            };
            if (element.Category == null)
            {
                return solids;
            }
            #region FOR CURTAIN WALLS

            if (element.Category.Name == "Walls")
            {
                Wall w = element as Wall;
                CurtainGrid grid = null;
                try
                {
                    grid = w.CurtainGrid;
                }
                catch { }
                if (grid != null)
                {
                    ICollection<ElementId> mullionIds = grid.GetMullionIds();
                    ICollection<ElementId> panelIds = grid.GetPanelIds();
                    if (mullionIds.Count > 0)
                    {
                        foreach (ElementId id in mullionIds)
                        {
                            Element ex = doc.GetElement(id);
                            GeometryElement geox = ex.get_Geometry(options);
                            foreach (GeometryObject geoObj in geox)
                            {
                                if (geoObj.IsElementGeometry && (geoObj as GeometryInstance) != null)
                                {
                                    foreach (GeometryObject geoObj1 in (geoObj as GeometryInstance).GetInstanceGeometry())
                                    {
                                        Solid solid = geoObj1 as Solid;
                                        if (solid != null && solid.Volume > Constants.VolumeTolerance)
                                        {

                                            solids.Add(solid);
                                        }
                                    }
                                }
                                else
                                {
                                    Solid solid = geoObj as Solid;
                                    if (solid != null && solid.Volume > Constants.VolumeTolerance)
                                    {

                                        solids.Add(solid);
                                    }
                                }
                            }
                        }
                    }
                    if (panelIds.Count > 0)
                    {
                        foreach (ElementId id in panelIds)
                        {
                            Element ex = doc.GetElement(id);
                            GeometryElement geox = ex.get_Geometry(options);
                            foreach (GeometryObject geoObj in geox)
                            {
                                if (geoObj.IsElementGeometry && (geoObj as GeometryInstance) != null)
                                {
                                    foreach (GeometryObject geoObj1 in (geoObj as GeometryInstance).GetInstanceGeometry())
                                    {
                                        Solid solid = geoObj1 as Solid;
                                        if (solid != null && solid.Volume > Constants.VolumeTolerance)
                                        {
                                            solids.Add(solid);
                                        }
                                    }
                                }
                                else
                                {
                                    Solid solid = geoObj as Solid;
                                    if (solid != null && solid.Volume > Constants.VolumeTolerance)
                                    {
                                        solids.Add(solid);
                                    }
                                }
                            }
                        }
                    }
                    return solids;
                }
            }
            #endregion
            try
            {
                GeometryElement geomElem = element.get_Geometry(options);

                if (geomElem == null)
                {
                    return null;
                }
                if (transform != null)
                {
                    geomElem = geomElem.GetTransformed(transform);
                }
                foreach (GeometryObject geomObj in geomElem)
                {
                    if (!check)
                    {
                        check = true;
                    }
                    if (geomObj is Solid)
                    {
                        Solid solid = (Solid)geomObj;
                        if (solid.Faces.Size > 0 && solid.Volume > Constants.VolumeTolerance)
                        {
                            solids.Add(solid);
                        }
                        // Single-level recursive check of instances. If viable solids are more than
                        // one level deep, this example ignores them.
                    }
                    else if (geomObj is GeometryInstance)
                    {
                        GeometryInstance geomInst = (GeometryInstance)geomObj;
                        GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                        foreach (GeometryObject geoObj2 in instGeomElem)
                        {

                            if (geoObj2 is Solid)
                            {
                                Solid solid = (Solid)geoObj2;

                                if (solid.Faces.Size > 0 && solid.Volume > Constants.VolumeTolerance)
                                {
                                    GraphicsStyle gStyle = doc.GetElement(geoObj2.GraphicsStyleId) as GraphicsStyle;
                                    if (gStyle != null && gStyle.Name == "Light Source")
                                    {
                                        //phần sáng của đèn
                                    }
                                    else
                                    {
                                        solids.Add(solid);
                                    }
                                }
                            }
                        }
                    }
                }
                if (check)
                {
                    solids = solids.OrderByDescending(x => x.Volume).ToList();
                    return solids;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return solids;
            }


        }

        public static Solid CreateSolidFromBoundingBox(this
            BoundingBoxXYZ bbox)
        {
            // Corners in BBox coords

            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            // Edges in BBox coords

            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);

            // Create loop, still in BBox coords

            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);

            double height = bbox.Max.Z - bbox.Min.Z;

            CurveLoop baseLoop = CurveLoop.Create(edges);

            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);

            Solid preTransformBox = GeometryCreationUtilities
                .CreateExtrusionGeometry(loopList, XYZ.BasisZ,
                    height);

            Solid transformBox = SolidUtils.CreateTransformed(
                preTransformBox, bbox.Transform);

            return transformBox;
        }

        // Thường lấy solid goc của Dầm (Chuẩn)

        public static Solid GetOriginSolidFramingSweptGeometry(this FamilyInstance framing, Document doc)
        {
            if (framing.Location is LocationCurve locationCurve)
            {
                Curve framingLocation = locationCurve.Curve;

                Solid solid = GetSolidOriginalFromFamilyInstance(framing);

                IEnumerable<PlanarFace> planarFaces = solid.Faces.OfType<PlanarFace>();

                if (planarFaces.Count() > 0)
                {
                    foreach (PlanarFace planarFace in planarFaces)
                    {
                        if (planarFace.FaceNormal.AngleBetweenTwoVectors(framingLocation.ComputeDerivatives(0, true).BasisX, false) < 1)
                        {
                            planarFace.Intersect(framingLocation, out IntersectionResultArray intersectionResultArray);
                            if (null != intersectionResultArray)
                            {
                                return GeometryCreationUtilities.CreateSweptGeometry(CurveLoop.Create(new List<Curve>() { framingLocation }), 0, intersectionResultArray.get_Item(0).Parameter, planarFace.GetEdgesAsCurveLoops());
                            }
                            intersectionResultArray?.Dispose();
                        }
                    }
                    foreach (PlanarFace planarFace in planarFaces)
                    {
                        if (planarFace.FaceNormal.AngleBetweenTwoVectors(framingLocation.ComputeDerivatives(1, true).BasisX, false) < 1)
                        {
                            planarFace.Intersect(framingLocation, out IntersectionResultArray intersectionResultArray);

                            if (null != intersectionResultArray)
                            {
                                return GeometryCreationUtilities.CreateSweptGeometry(

                                                                                         CurveLoop.Create(new List<Curve>() { framingLocation })

                                                                                         , 0
                                                                                         , intersectionResultArray.get_Item(0).Parameter
                                                                                         , planarFace.GetEdgesAsCurveLoops()

                                                                                     );
                            }
                            intersectionResultArray?.Dispose();
                        }
                    }
                }
            }

            return null;
        }

        // Thương lấy solid goc cho cột (Trương hợp này lấy cột luôn đúng)
        public static Solid GetSolidOriginalFromFamilyInstance(this FamilyInstance Fami)
        {
            var tranf = Fami.GetTotalTransform();

            Solid SolidColumns = null;

            foreach (var Geo in Fami.GetOriginalGeometry(new Options()))
            {
                if (Geo is Solid Soli)
                {
                    Soli = SolidUtils.CreateTransformed(Soli, tranf);

                    if (SolidColumns == null)
                    {
                        SolidColumns = Soli;
                    }
                    else
                    {
                        SolidColumns = SolidColumns.UnionSoild(Soli);
                    }
                }
            }
            return SolidColumns;
        }

        public static Solid GetSolidOriginalWall(this Wall wall)
        {
            Curve curveWall = (wall.Location as LocationCurve).Curve;

            var baseOffset = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();

            var vectorWall = Transform.CreateTranslation(XYZ.BasisZ * baseOffset);

            curveWall = curveWall.CreateTransformed(vectorWall);

            var Curloop = CurveLoop.CreateViaThicken(curveWall, wall.Width, XYZ.BasisZ);

            var Height = wall.LookupParameter("Unconnected Height").AsDouble();

            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { Curloop }, XYZ.BasisZ, Height);

            return solid;
        }

        // Lấy solid góc cho tường : Bung đương curve của tương ra thành curveloop rồi extrus
        public static Solid GetSolidOriginalWall(this Element ele)
        {
            Wall locationCurveWall = (ele as Wall);

            Curve curveWall = (locationCurveWall.Location as LocationCurve).Curve;

            var baseOffset = locationCurveWall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble();

            var vectorWall = Transform.CreateTranslation(XYZ.BasisZ * baseOffset);

            curveWall = curveWall.CreateTransformed(vectorWall);

            var Curloop = CurveLoop.CreateViaThicken(curveWall, locationCurveWall.Width, XYZ.BasisZ);

            var Height = locationCurveWall.LookupParameter("Unconnected Height").AsDouble();

            var solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { Curloop }, XYZ.BasisZ, Height);

            return solid;
        }

        // Lấy solid goc bằng cách unjon (khá chậm)
        public static Solid GetSlowSolid(this Element e, Document doc)
        {
            Solid soild = null;

            using (SubTransaction t = new SubTransaction(AC.Document))
            {
                t.Start();

                try
                {
                    if (e.Category.Name.Contains("Framing") && e is FamilyInstance Fami)
                    {
                        if (!Fami.Symbol.Family.IsInPlace)
                        {
                            if (StructuralFramingUtils.IsJoinAllowedAtEnd(Fami, 0))
                            {
                                StructuralFramingUtils.DisallowJoinAtEnd(Fami, 0);
                            }
                            if (StructuralFramingUtils.IsJoinAllowedAtEnd(Fami, 1))
                            {
                                StructuralFramingUtils.DisallowJoinAtEnd(Fami, 1);
                            }
                        }
                    }

                    if (e.Category.Name.Contains("Walls") && e is Wall wall)
                    {
                        if (WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
                        {
                            WallUtils.DisallowWallJoinAtEnd(wall, 0);
                        }

                        if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
                        {
                            WallUtils.DisallowWallJoinAtEnd(wall, 1);
                        }
                    }

                    ICollection<ElementId> Listelejoin = JoinGeometryUtils.GetJoinedElements(doc, e);

                    if (Listelejoin.Count > 0)
                    {
                        foreach (ElementId i in Listelejoin)
                        {
                            var EleJoin = doc.GetElement(i);

                            JoinGeometryUtils.UnjoinGeometry(doc, e, EleJoin);
                        }
                    }

                    AC.Document.Regenerate();

                    soild = SolidUtils.Clone(e.GetAllSolids().UnionSoilds());

                    t.RollBack();

                    t.Dispose();
                }
                catch
                {
                    // cùng đường thì lấy solid thật
                    MessageBox.Show("Lỗi lấy solid thật khi dùng GetSlowSolid vui lòng báo lại");

                    e.GetAllSolids().UnionSoilds();
                }
            }
            return soild;
        }

        public static Solid UnionSoild(this List<Solid> solids)
        {
            Solid SoliTM = null;
            foreach (var soli in solids)
            {
                if (soli == null)
                {
                    return null;
                }

                if (SoliTM == null)
                {
                    SoliTM = soli;
                }
                else
                {
                    SoliTM = BooleanOperationsUtils.ExecuteBooleanOperation(soli, SoliTM, BooleanOperationsType.Union);
                }
            }
            return SoliTM;
        }

        public static Solid UnionSoild(this Solid solid1, Solid soild2)
        {
            if (solid1 == null || solid1 == null)
            {
                return null;
            }
            return BooleanOperationsUtils.ExecuteBooleanOperation(solid1, soild2, BooleanOperationsType.Union);
        }

        public static Solid CutSolid(this Solid SolidOrigin, Solid SolidCut)
        {
            try
            {
                BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(SolidOrigin, SolidCut, BooleanOperationsType.Difference);
            }
            catch
            {
                // show loi
            }

            return SolidOrigin;
        }

        static public Solid Clone(this Solid solid)
        {
            return SolidUtils.Clone(solid);
        }

        public static Solid GetSolidZero()
        {
            var Cr = new CurveLoop();

            Cr.Append(Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 10, 0)));

            Cr.Append(Line.CreateBound(new XYZ(0, 10, 0), new XYZ(10, 10, 0)));

            Cr.Append(Line.CreateBound(new XYZ(10, 10, 0), new XYZ(10, 0, 0)));

            Cr.Append(Line.CreateBound(new XYZ(10, 0, 0), new XYZ(0, 0, 0)));

            var soild = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop>() { Cr }, -XYZ.BasisZ, 5);

            return BooleanOperationsUtils.ExecuteBooleanOperation(soild, soild, BooleanOperationsType.Difference);
        }

        public static Solid UnionSoilds(this List<Solid> solids)
        {
            Solid SoliTM = null;

            foreach (var soli in solids)
            {
                if (soli == null || soli.Volume < 0.01) continue;

                if (SoliTM == null)
                {
                    SoliTM = soli;
                }
                else
                {
                    SoliTM = BooleanOperationsUtils.ExecuteBooleanOperation(soli, SoliTM, BooleanOperationsType.Union);
                }
            }
            return SoliTM;
        }

        public static Solid GetSolidFromMinMaxPoints(XYZ minPoint, XYZ maxPoint)
        {
            if (minPoint.X == maxPoint.X || minPoint.Y == maxPoint.Y || minPoint.Z == maxPoint.Z)
            {
                XYZ point0 = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
                XYZ point1 = new XYZ(maxPoint.X, minPoint.Y, minPoint.Z);
                XYZ point2 = new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z);
                XYZ point3 = new XYZ(minPoint.X, maxPoint.Y, minPoint.Z);
                Line line0 = Line.CreateBound(point0, point1);
                Line line1 = Line.CreateBound(point1, point2);
                Line line2 = Line.CreateBound(point2, point3);
                Line line3 = Line.CreateBound(point3, point0);
                List<Curve> curves = new List<Curve>() { line0, line1, line2, line3 };
                List<CurveLoop> curveLoops = new List<CurveLoop>() { CurveLoop.Create(curves) };
                return GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, XYZ.BasisZ, maxPoint.Z - minPoint.Z);
            }

            return null;
        }

        public static IList<Solid> GetAllSolids(this FamilyInstance instance, out Transform transform)
        {
            transform = Transform.CreateTranslation(XYZ.Zero);
            List<Solid> solidList = new List<Solid>();
            if (instance == null)
                return solidList;
            GeometryElement geometryElement = instance.get_Geometry(new Options()
            {
                ComputeReferences = true
            });
            List<GeometryObject> geometryObjectList = new List<GeometryObject>();
            bool flag = false;
            foreach (GeometryObject geometryObject1 in geometryElement)
            {
                geometryObjectList.Add(geometryObject1);
                GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
                if (null != geometryInstance)
                {
                    var tf = geometryInstance.Transform;
                    foreach (GeometryObject geometryObject2 in geometryInstance.GetSymbolGeometry())
                    {
                        Solid solid = geometryObject2 as Solid;
                        if (!(null == solid) && solid.Faces.Size != 0 && solid.Edges.Size != 0)
                        {
                            solidList.Add(SolidUtils.CreateTransformed(solid, tf));
                        }
                    }
                }
                Solid solid1 = geometryObject1 as Solid;
                if (!(null == solid1) && solid1.Faces.Size != 0 && solid1.Edges.Size != 0)
                    solidList.Add(solid1);
            }
            if (flag)
                transform = instance.GetTransform();
            return solidList;
        }

        public static List<Solid> GetAllSolids(this Element instance, bool transformedSolid = false, View view = null)
        {
            List<Solid> solidList = new List<Solid>();
            if (instance == null)
                return solidList;
            GeometryElement geometryElement = instance.get_Geometry(new Options()
            {
                ComputeReferences = true
            });

            foreach (GeometryObject geometryObject1 in geometryElement)
            {
                GeometryInstance geometryInstance = geometryObject1 as GeometryInstance;
                if (null != geometryInstance)
                {
                    var tf = geometryInstance.Transform;
                    foreach (GeometryObject geometryObject2 in geometryInstance.GetSymbolGeometry())
                    {
                        Solid solid = geometryObject2 as Solid;
                        if (!(null == solid) && solid.Faces.Size != 0 && solid.Edges.Size != 0)
                        {
                            if (transformedSolid)
                            {
                                solidList.Add(SolidUtils.CreateTransformed(solid, tf));
                            }
                            solidList.Add(solid);
                        }
                    }
                }
                Solid solid1 = geometryObject1 as Solid;
                if (!(null == solid1) && solid1.Faces.Size != 0)
                    solidList.Add(solid1);
            }
            return solidList;
        }

        public static Solid GetSingleSolid(this Element e)
        {
            Solid s = null;
            foreach (GeometryObject geoObj in e.get_Geometry(new Options() { ComputeReferences = true }))
            {
                if (geoObj is Solid)
                {
                    s = geoObj as Solid;
                    if (s.Faces.Size != 0 && s.Edges.Size != 0) goto L1;
                }
                if (geoObj is GeometryInstance)
                    foreach (GeometryObject geoObj2 in (geoObj as GeometryInstance).GetInstanceGeometry())
                    {
                        s = geoObj2 as Solid;
                        if (s != null)
                            if (s.Faces.Size != 0 && s.Edges.Size != 0) goto L1;
                    }
            }
        L1:
            return s;
        }

        /// <summary>
        /// Kết hợp nhiều soild của các element
        /// </summary>

        public static Solid CreateMergeSolid(this IEnumerable<Element> elems)
        {
            Solid fullSolid = null;
            try
            {
                foreach (var elem in elems)
                {
                    if (fullSolid == null)
                    {
                        fullSolid = elem.GetAllSolids().First();
                    }
                    else
                    {
                        fullSolid = BooleanOperationsUtils.ExecuteBooleanOperation(fullSolid, elem.GetAllSolids().First(), BooleanOperationsType.Union);
                    }
                }
            }
            catch
            {
                throw;
            }
            return fullSolid;
        }

        /// <summary>

        public static Solid ScaleSolid(this Solid solid, double x)
        {
            XYZ centralPnt = solid.ComputeCentroid();
            Transform tf = Transform.Identity;
            tf.BasisX = XYZ.BasisX * x;
            tf.BasisY = XYZ.BasisY * x;
            tf.BasisZ = XYZ.BasisZ * x;

            Solid s = SolidUtils.CreateTransformed(solid, tf);
            tf = Transform.CreateTranslation(centralPnt - s.ComputeCentroid());
            s = SolidUtils.CreateTransformed(solid, tf);
            return s;
        }

        public static BoundingBoxXYZ ScaleBoundingBox(this BoundingBoxXYZ bb, double x)
        {
            var min = bb.Min; var max = bb.Max; var origin = (min + max) / 2;
            return new BoundingBoxXYZ { Min = origin + (min - origin) * x, Max = origin + (max - origin) * x };
        }
    }
}