#region Namespaces

using Autodesk.Revit.DB;
using HcBimUtils.DocumentUtils;
using HcBimUtils.MoreLinq;

#endregion Namespaces

namespace HcBimUtils.GeometryUtils
{
    /// <summary>
    /// Kiểu dữ liệu chứa các phương thức tĩnh xử lý các đối tượng hình học Revit
    /// </summary>
    public static class GeometryUtils
    {
        public static Level GetNearestLevel(this double z, List<Level> levels)
        {
            var ordered = levels.OrderBy(level => level.Elevation).ToList();

            var level = ordered.MinBy2(x => Math.Abs(x.Elevation - z));
            return level;
        }

        public static Solid SolidSubtractSolids(this Solid origin, List<Solid> subtractSolids)
        {
            foreach (var subtract in subtractSolids)
            {
                SolidSubtractSolid(origin, subtract);
            }

            return origin;
        }

        public static Solid SolidSubtractSolid(this Solid origin, Solid solid)
        {
            if (solid == null) return origin;
            try
            {
                BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(origin, solid, BooleanOperationsType.Difference);
            }
            catch
            {
                // ignored
            }

            return origin;
        }

        public static DirectShape ModelSolid(this Solid solid, Document doc, DirectShapeTypeEnum typeEnum, ElementId materiaElementId = null)
        {
            if (solid == null) return null;
            var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_SpecialityEquipment));
            ds.SetShape(new GeometryObject[] { solid });
            var id = typeEnum.GetDirectShapeId();
            ds.SetTypeId(id);
            if (materiaElementId == null) return ds;
            try
            {
                Paint(ds, materiaElementId);
            }
            catch
            {
                // ignored
            }

            return ds;
        }

        public static ElementId GetDirectShapeId(this DirectShapeTypeEnum type)
        {
            var id = ElementId.InvalidElementId;
            var dsTypes = new FilteredElementCollector(AC.Document).OfClass(typeof(DirectShapeType)).Cast<DirectShapeType>().ToList();
            foreach (var dsType in dsTypes.Where(dsType => dsType.Name == type.ToString()))
            {
                id = dsType.Id;
            }

            if (id != ElementId.InvalidElementId) return id;
            var newDirectShapeType = DirectShapeType.Create(AC.Document, type.ToString(), new ElementId(BuiltInCategory.OST_SpecialityEquipment));
            id = newDirectShapeType.Id;
            return id;
        }

        public static Solid CreateCube2(XYZ centerBot, double d, XYZ direct, double thick)
        {
            var list = new List<Curve>();
            var A = centerBot.Add(XYZ.BasisY * d * 0.5).Add(XYZ.BasisX * -d * 0.5);
            var B = centerBot.Add(XYZ.BasisY * d * 0.5).Add(XYZ.BasisX * d * 0.5);
            var C = centerBot.Add(XYZ.BasisY * d * -0.5).Add(XYZ.BasisX * d * 0.5);
            var D = centerBot.Add(XYZ.BasisY * -d * 0.5).Add(XYZ.BasisX * -d * 0.5);
            list.Add(Line.CreateBound(A, B));
            list.Add(Line.CreateBound(B, C));
            list.Add(Line.CreateBound(C, D));
            list.Add(Line.CreateBound(D, A));
            var curveLoop = CurveLoop.Create(list);
            var solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new CurveLoop[1] { curveLoop }, direct, thick, solidOptions);
        }

        public static void Paint(DirectShape ds, ElementId materialId)
        {
            AC.Document.Regenerate();
            var faces = ds.AllFaces();
            foreach (var face in faces)
            {
                AC.Document.Paint(ds.Id, face, materialId);
            }
        }

        public static double PerimeterCurveLoop(this CurveLoop cl)
        {
            return cl.Sum(curve => curve.ApproximateLength);
        }

        public static Solid SolidByCylindricalFace(this CylindricalFace face, double thickness, double height)
        {
            var s = CreateRectangular(1, 1, 1, XYZ.BasisZ);
            BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, s, BooleanOperationsType.Difference);
            try
            {
                foreach (var curveLoop in face.GetEdgesAsCurveLoops())
                {
                    var min = (from curve in curveLoop.OfType<Arc>() select curve.Midpoint() into mid select mid.Z).Prepend(double.MaxValue).Min();
                    var arcs = (from curve in curveLoop.OfType<Arc>() let mid = curve.Midpoint() where mid.Z.IsEqual(min, 0.001) select curve as Arc).ToList();
                    foreach (var arc in arcs)
                    {
                        if (!(height > 0) || arc == null) continue;
                        Arc thisArc;
                        var mid = arc.Evaluate(0.5, false);
                        var normalFace = face.ComputeNormal(new UV(0.5, 0.5));
                        var arc2 = arc.CreateOffset(thickness, XYZ.BasisZ) as Arc;
                        var mid2 = arc2.Evaluate(0.5, false);
                        var arc3 = arc.CreateOffset(thickness, -XYZ.BasisZ) as Arc;
                        var mid3 = arc3.Evaluate(0.5, false);

                        var vector21 = mid2 - mid;
                        thisArc = vector21.DotProduct(normalFace) > 0 ? arc2 : arc3;
                        var listCurveloop = new List<CurveLoop>();
                        var cl = new CurveLoop();
                        cl.Append(thisArc);
                        cl.Append(Line.CreateBound(thisArc.EP(), arc.EP()));
                        cl.Append(arc.CreateReversed());
                        cl.Append(Line.CreateBound(arc.SP(), thisArc.SP()));
                        listCurveloop.Add(cl);
                        var solid = GeometryCreationUtilities.CreateExtrusionGeometry(listCurveloop, XYZ.BasisZ, height);
                        BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(s, solid, BooleanOperationsType.Union);
                    }
                }
            }
            catch
            {
                // ignored
            }

            return s;
        }

        public static Solid CombineSolidInOne(List<Solid> solids)
        {
            if (solids.Count == 0) return null;
            var solid = CreateRectangular(1, 1, 1, XYZ.BasisZ);
            BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid, solid, BooleanOperationsType.Difference);
            foreach (var item in solids)
            {
                try
                {
                    BooleanOperationsUtils.ExecuteBooleanOperationModifyingOriginalSolid(solid, item, BooleanOperationsType.Union);
                }
                catch
                {
                    // ignored
                }
            }

            return solid;
        }

        public static Solid CreateRectangular(double d1, double d2, double d3, XYZ direct)
        {
            var list = new List<Curve>();
            var xYZ = new XYZ((0.0 - d1) / 2.0, (0.0 - d2) / 2.0, (0.0 - d3) / 2.0);
            var xYZ2 = new XYZ((0.0 - d1) / 2.0, d2 / 2.0, (0.0 - d3) / 2.0);
            var xYZ3 = new XYZ(d1 / 2.0, d2 / 2.0, (0.0 - d3) / 2.0);
            var xYZ4 = new XYZ(d1 / 2.0, (0.0 - d2) / 2.0, (0.0 - d3) / 2.0);
            list.Add(Line.CreateBound(xYZ, xYZ2));
            list.Add(Line.CreateBound(xYZ2, xYZ3));
            list.Add(Line.CreateBound(xYZ3, xYZ4));
            list.Add(Line.CreateBound(xYZ4, xYZ));
            var curveLoop = CurveLoop.Create(list);
            var solidOptions = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            return GeometryCreationUtilities.CreateExtrusionGeometry(new CurveLoop[1] { curveLoop }, direct, d3, solidOptions);
        }

        public static List<PlanarFace> FacesBySymbol(this FamilyInstance familyInstance, bool transformSolid = false)
        {
            var faces = new List<PlanarFace>();
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var geoI = geoO as GeometryInstance;
                if (geoI == null) continue;
                var instanceGeoE = geoI.GetSymbolGeometry();
                var tf = geoI.Transform;
                foreach (var instanceGeoObj in instanceGeoE)
                {
                    var solid1 = instanceGeoObj as Solid;
                    var solid = solid1;
                    if (transformSolid)
                    {
                        solid = SolidUtils.CreateTransformed(solid1, tf);
                    }

                    if (solid == null || solid.Faces.Size == 0) continue;
                    faces.AddRange(solid.Faces.OfType<PlanarFace>());
                }
            }

            return faces;
        }

        public static List<Face> FacesBySymbol(this Element familyInstance, bool transformSolid = false, bool include = true)
        {
            var faces = new List<Face>();
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = false;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var geoI = geoO as GeometryInstance;
                if (geoI == null) continue;
                var instanceGeoE = geoI.GetSymbolGeometry();
                var tf = geoI.Transform;
                foreach (var instanceGeoObj in instanceGeoE)
                {
                    var solid1 = instanceGeoObj as Solid;
                    var solid = solid1;
                    if (transformSolid)
                    {
                        solid = SolidUtils.CreateTransformed(solid1, tf);
                    }

                    if (solid == null || solid.Faces.Size == 0) continue;
                    foreach (Face face in solid.Faces)
                    {
                        var planarFace = face as PlanarFace;
                        if (planarFace != null)
                        {
                            faces.Add(planarFace);
                        }

                        var cy = face as CylindricalFace;
                        if (cy != null)
                        {
                            faces.Add(cy);
                        }
                    }
                }
            }

            return faces;
        }

        public static List<Face> FacesBySymbol(this Element familyInstance)
        {
            var faces = new List<Face>();
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            faces.AddRange((from geoI in geoE.OfType<GeometryInstance>() let instanceGeoE = geoI.GetSymbolGeometry() let tf = geoI.Transform from instanceGeoObj in instanceGeoE let solid1 = instanceGeoObj as Solid select SolidUtils.CreateTransformed(solid1, tf) into solid where solid != null && solid.Faces.Size != 0 from Face face in solid.Faces select face).OfType<Face>());
            return faces;
        }

        public static List<Face> FacesBySymbolAndView(this Element familyInstance, View view, out Transform tf)
        {
            tf = Transform.Identity;

            var faces = new List<Face>();
            var op = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true, View = view };
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var geoI = geoO as GeometryInstance;
                if (geoI == null) continue;
                var instanceGeoE = geoI.GetSymbolGeometry();
                tf = geoI.Transform;
                foreach (var instanceGeoObj in instanceGeoE)
                {
                    var solid1 = instanceGeoObj as Solid;
                    var solid = SolidUtils.CreateTransformed(solid1, tf);
                    if (solid == null || solid.Faces.Size == 0) continue;
                    faces.AddRange(solid.Faces.OfType<Face>());
                }
            }

            return faces;
        }

        public static List<PlanarFace> Faces(this FamilyInstance fi)
        {
            var faces = new List<PlanarFace>();
            // Option :
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = fi.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                faces.AddRange(solid.Faces.OfType<PlanarFace>());
            }

            return faces;
        }

        public static List<Line> Lines(this Element fi)
        {
            var lines = new List<Line>();
            // Option :
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            //op.DetailLevel = ViewDetailLevel.Coarse;
            op.View = fi.Document.ActiveView;
            var geoE = fi.get_Geometry(op);
            if (geoE == null) return lines;
            foreach (var geoO in geoE)
            {
                if (geoO is Line line)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public static List<Line> Lines(this Element fi, View view)
        {
            var lines = new List<Line>();
            // Option :
            var op = new Options { ComputeReferences = true, IncludeNonVisibleObjects = true, View = fi.Document.ActiveView };
            op.View = view;
            var geoE = fi.get_Geometry(op);
            if (geoE == null) return lines;
            foreach (var geoO in geoE)
            {
                if (geoO is Line line)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public static List<Face> Faces(this Element fi, bool include = true)
        {
            var faces = new List<Face>();
            // Option :
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = false;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = fi.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                foreach (var f in solid.Faces)
                {
                    var planarFace = f as PlanarFace;
                    if (planarFace != null)
                    {
                        faces.Add(planarFace);
                    }

                    var cy = f as CylindricalFace;
                    if (cy != null)
                    {
                        faces.Add(cy);
                    }
                }
            }

            return faces;
        }


        public static List<Face> FacesByView(this Element fi, View view = null)
        {
            var faces = new List<Face>();
            // Option :
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = false;
            if (view != null)
            {
                op.View = view;
            }

            var geoE = fi.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                foreach (var f in solid.Faces)
                {
                    var planarFace = f as PlanarFace;
                    if (planarFace != null)
                    {
                        faces.Add(planarFace);
                    }

                    var cy = f as CylindricalFace;
                    if (cy != null)
                    {
                        faces.Add(cy);
                    }
                }
            }

            return faces;
        }

        public static List<Face> Faces(this Element fi)
        {
            var faces = new List<Face>();
            // Option :
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = fi.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                faces.AddRange(solid.Faces.OfType<Face>());
            }

            return faces;
        }

        public static List<PlanarFace> AllFaces(this FamilyInstance fi, out Transform tf)
        {
            tf = Transform.Identity;
            var faces = fi.Faces();
            if (faces.Count != 0) return faces;
            faces = fi.FacesBySymbol();
            tf = fi.GetTransform();
            return faces;
        }

        public static List<Face> AllFaces(this Element fi, bool transformSolid = false, bool include = true)
        {
            var faces = fi.Faces(include);
            if (faces.Count == 0)
            {
                faces = fi.FacesBySymbol(transformSolid, include);
            }

            return faces;
        }

        public static List<Face> AllFaces(this Element fi, out Transform transform, bool include = true)
        {
            transform = Transform.Identity;
            var faces = fi.Faces(include);
            if (faces.Count != 0) return faces;
            transform = (fi as FamilyInstance)?.GetTransform();
            faces = fi.FacesBySymbol(false, include);
            return faces;
        }

        public static bool IsPerpendicular(this PlanarFace face, XYZ vector)
        {
            var flag = false;
            var faceNormal = face.FaceNormal;
            if ((vector.CrossProduct(faceNormal) / (vector.GetLength() * faceNormal.GetLength())).GetLength() < 1E-05)
                flag = true;
            return flag;
        }

        public static bool IsPerpendicular(PlanarFace face, XYZ vector, Transform transform)
        {
            var flag = false;
            var faceNormal = face.FaceNormal;
            var source = transform.OfVector(faceNormal);
            if ((vector.CrossProduct(source) / (vector.GetLength() * source.GetLength())).GetLength() < 0.001)
                flag = true;
            return flag;
        }

        public static XYZ CenterPoint(PlanarFace face)
        {
            var xyz = new XYZ();
            var mesh = face.Triangulate();
            IList<XYZ> xyzList = new List<XYZ>();
            foreach (var vertex in mesh.Vertices)
            {
                xyz += vertex;
                xyzList.Add(vertex);
            }

            return xyz / xyzList.Count;
        }

        public static IEnumerable<PlanarFace> PerpendicularFace(IEnumerable<PlanarFace> listFaces, XYZ vector)
        {
            return listFaces.Where(listFace => IsPerpendicular(listFace, vector)).ToList();
        }

        public static IEnumerable<PlanarFace> PerpendicularFace(IEnumerable<PlanarFace> listFaces, XYZ vector, Transform transform)
        {
            return listFaces.Where(listFace => IsPerpendicular(listFace, vector, transform)).ToList();
        }

        public static PlanarFace FirstFace(this XYZ vector, List<PlanarFace> listFace, bool trueReference = true)
        {
            PlanarFace planarFace = null;
            var planarFaceList = PerpendicularFace(listFace, vector);
            var d1 = 0.0;
            foreach (var face in planarFaceList)
            {
                if (face.Reference == null && trueReference)
                {
                    continue;
                }

                var source = CenterPoint(face);
                if (d1.IsEqual(0.0, 0.0001))
                {
                    d1 = vector.DotProduct(source);
                    planarFace = face;
                }
                else
                {
                    var num = vector.DotProduct(source);
                    if (!(num < d1)) continue;
                    planarFace = face;
                    d1 = num;
                }
            }

            return planarFace;
        }

        public static PlanarFace FirstFace(this XYZ vector, IEnumerable<PlanarFace> listFace, Transform transform, bool trueReference = true)
        {
            var faces = listFace.Where(x => transform.OfVector(x.FaceNormal).IsParallel(vector)).OrderBy(x => x.Origin.DotProduct(vector)).ToList();
            if (trueReference)
            {
                faces = faces.Where(x => x.Reference != null).ToList();
            }

            return faces.FirstOrDefault();
        }

        public static PlanarFace LastFace(this IEnumerable<PlanarFace> listFace, XYZ vector)
        {
            PlanarFace planarFace1 = null;
            var planarFaceList = PerpendicularFace(listFace, vector);
            var d1 = 0.0;
            foreach (var planarFace2 in planarFaceList)
            {
                var origin = planarFace2.Origin;
                if (d1.IsEqual(0.0, 0.0001))
                {
                    d1 = vector.DotProduct(origin);
                    planarFace1 = planarFace2;
                }
                else
                {
                    var num = vector.DotProduct(origin);
                    if (!(num > d1)) continue;
                    planarFace1 = planarFace2;
                    d1 = num;
                }
            }

            return planarFace1;
        }

        public static PlanarFace LastFace(this IEnumerable<PlanarFace> listFace, XYZ vector, Transform transform)
        {
            PlanarFace planarFace1 = null;
            var planarFaceList = PerpendicularFace(listFace, vector, transform).ToList();
            var max = double.MinValue;
            foreach (var planarFace2 in planarFaceList)
            {
                var origin = planarFace2.Origin;
                var source = transform.OfPoint(origin);

                var num = vector.DotProduct(source);
                if (!(num > max)) continue;
                max = num;
                planarFace1 = planarFace2;
            }

            return planarFace1;
        }

        public static double PointToFace(XYZ point, PlanarFace face)
        {
            var faceNormal = face.FaceNormal;
            return Math.Abs((point - face.Origin).DotProduct(faceNormal) / faceNormal.GetLength());
        }

        public static IList<Edge> AllEdges(FamilyInstance familyInstance)
        {
            var options = new Options();
            options.ComputeReferences = true;
            return (from geometryObject in familyInstance.get_Geometry(options) select geometryObject as Solid into solid where null != solid && solid.Faces.Size != 0 && solid.Edges.Size != 0 from Edge edge in solid.Edges select edge).ToList();
        }

        public static IList<Edge> AllEdgesBySymbol(FamilyInstance familyInstance)
        {
            var options = new Options();
            options.ComputeReferences = true;
            return (from geometryInstance in familyInstance.get_Geometry(options).OfType<GeometryInstance>() from geometryObject2 in geometryInstance.GetSymbolGeometry() select geometryObject2 as Solid into solid where null != solid && solid.Faces.Size != 0 && solid.Edges.Size != 0 from Edge edge in solid.Edges where edge != null select edge).ToList();
        }

        public static List<Line> GetInsideLinesIntersectSolids(this Line line, List<Solid> solids)
        {
            var lines = new List<Line>();
            var option = new SolidCurveIntersectionOptions() { ResultType = SolidCurveIntersectionMode.CurveSegmentsInside };
            foreach (var intersection in from solid in solids select solid.IntersectWithCurve(line, option))
            {
                intersection.Cast<Line>().ToList().ForEach(x => lines.Add(x));
            }

            return lines;
        }

        public static List<Line> LinesDivideBySolid(this List<Line> lines, Solid solid)
        {
            var list = new List<Line>();
            foreach (var linesOutSide in from line in lines select line.LineDivideBySolid(solid))
            {
                linesOutSide.ForEach(x => list.Add(x));
            }

            return list;
        }

        public static List<Line> LineDivideBySolid(this Line line, Solid solid)
        {
            var rt = solid.IntersectWithCurve(line, new SolidCurveIntersectionOptions { ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside });
            return rt.Flatten().Cast<Line>().ToList();
        }

        public static List<Curve> CurvesDivideBySolid(this List<Curve> curves, Solid solid)
        {
            var list = new List<Curve>();
            foreach (var linesOutSide in from line in curves select line.CurveDivideBySolid(solid))
            {
                linesOutSide.ForEach(x => list.Add(x));
            }

            return list;
        }

        public static List<Curve> CurveDivideBySolid(this Curve line, Solid solid)
        {
            var rt = solid.IntersectWithCurve(line, new SolidCurveIntersectionOptions { ResultType = SolidCurveIntersectionMode.CurveSegmentsOutside });
            return rt.Flatten().Cast<Curve>().ToList();
        }

        public static IEnumerable<Solid> SplitSolid(this Solid solid)
        {
            var list = new List<Solid>();
            try
            {
                var solids = SolidUtils.SplitVolumes(solid);
                list.AddRange(solids);
            }
            catch
            {
                list.Add(solid);
            }

            return list;
        }

        public static List<Solid> SplitSolids(this List<Solid> solids)
        {
            var list = new List<Solid>();

            foreach (var solids2 in from solid in solids select solid.SplitSolid())
            {
                list.AddRange(solids2);
            }

            return list;
        }

        /// <summary>
        /// Sai số tọa độ cho phép trong Revit, nếu sai số nhỏ hơn sai số trên thì Revit hiểu là trùng nhau
        /// </summary>
        private const double Precision = 0.00001;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang meter
        /// </summary>
        private const double FEET_TO_METERS = 0.3048;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang centimeter
        /// </summary>
        private const double FEET_TO_CENTIMETERS = FEET_TO_METERS * 100;

        /// <summary>
        /// Hệ số chuyển đổi từ feet sang milimeter
        /// </summary>
        private const double FEET_TO_MILIMETERS = FEET_TO_METERS * 1000;

        /// <summary>
        /// Hàm chuyển đổi từ feet sang meter
        /// </summary>
        /// <param name="feet">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Feet2Meter(double feet)
        {
            return feet * FEET_TO_METERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ feet sang milimeter
        /// </summary>
        /// <param name="feet">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Feet2Milimeter(double feet)
        {
            return feet * FEET_TO_MILIMETERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ feet sang meter
        /// </summary>
        /// <param name="meter">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Meter2Feet(double meter)
        {
            return meter / FEET_TO_METERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ milimeter sang feet
        /// </summary>
        /// <param name="milimeter">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Milimeter2Feet(double milimeter)
        {
            return milimeter / FEET_TO_MILIMETERS;
        }

        /// <summary>
        /// Hàm chuyển đổi từ độ Radian sang độ Degree
        /// </summary>
        /// <param name="rad">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Radian2Degree(double rad)
        {
            return rad * 180 / Math.PI;
        }

        /// <summary>
        /// Hàm chuyển đổi từ độ Degree sang độ Radian
        /// </summary>
        /// <param name="deg">Giá trị đang xét</param>
        /// <returns></returns>
        public static double Degree2Radian(double deg)
        {
            return deg * Math.PI / 180;
        }

        /// <summary>
        /// Kiểm tra 2 giá trị có bằng nhau hay không trong môi trường Revit
        /// True: bằng nhau, False: khác nhau
        /// </summary>
        /// <param name="d1">Giá trị 1</param>
        /// <param name="d2">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsEqual(double d1, double d2)
        {
            //get the absolute value;
            var diff = Math.Abs(d1 - d2);
            return diff < Precision;
        }

        /// <summary>
        /// Kiểm tra 2 điểm, vector có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsEqual(XYZ first, XYZ second)
        {
            var flag = true;
            flag = IsEqual(first.X, second.X);
            flag = flag && IsEqual(first.Y, second.Y);
            flag = flag && IsEqual(first.Z, second.Z);
            return flag;
        }

        /// <summary>
        /// Kiểm tra 2 điểm, vector 2d có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Điểm, vector 2d 1</param>
        /// <param name="second">Điểm, vector 2d 2</param>
        /// <returns></returns>
        public static bool IsEqual(UV first, UV second)
        {
            var flag = true;
            flag = IsEqual(first.U, second.U);
            flag = flag && IsEqual(first.V, second.V);
            return flag;
        }

        /// <summary>
        /// Kiểm tra 2 đoạn thẳng có giống nhau hay không trong môi trường Revit
        /// True: giống nhau, False: khác nhau
        /// </summary>
        /// <param name="first">Đoạn thẳng 1</param>
        /// <param name="second">Đoạn thẳng 2</param>
        /// <returns></returns>
        public static bool IsEqual(Curve first, Curve second)
        {
            if (IsEqual(first.GetEndPoint(0), second.GetEndPoint(0)))
            {
                return IsEqual(first.GetEndPoint(1), second.GetEndPoint(1));
            }

            return IsEqual(first.GetEndPoint(1), second.GetEndPoint(0)) && IsEqual(first.GetEndPoint(0), second.GetEndPoint(1));
        }

        /// <summary>
        /// Kiểm tra điểm, vector 1 có nhỏ hơn điểm, vector 2 hay không trong môi trường Revit
        /// Cách thức so sánh theo thứ tự Z -> Y -> X
        /// True: nhỏ hơn, False: lớn hơn hoặc bằng
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsSmaller(XYZ first, XYZ second)
        {
            if (IsEqual(first, second)) return false;
            if (!IsEqual(first.Z, second.Z)) return first.Z < second.Z;
            if (IsEqual(first.Y, second.Y))
            {
                return first.X < second.X;
            }

            return first.Y < second.Y;
        }

        /// <summary>
        /// Kiểm tra giá trị 1 có nhỏ hơn giá trị 2 hay không trong môi trường Revit
        /// True: nhỏ hơn, False: lớn hơn hoặc bằng
        /// </summary>
        /// <param name="x">Giá trị 1</param>
        /// <param name="y">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsSmaller(double x, double y)
        {
            if (IsEqual(x, y)) return false;
            return x < y;
        }

        /// <summary>
        /// Kiểm tra điểm, vector 1 có lớn hơn điểm, vector 2 hay không trong môi trường Revit
        /// Cách thức so sánh theo thứ tự Z -> Y -> X
        /// True: lớn hơn, False: nhỏ hơn hoặc bằng
        /// </summary>
        /// <param name="first">Điểm, vector 1</param>
        /// <param name="second">Điểm, vector 2</param>
        /// <returns></returns>
        public static bool IsBigger(XYZ first, XYZ second)
        {
            if (IsEqual(first, second)) return false;
            if (!IsEqual(first.Z, second.Z)) return first.Z > second.Z;
            if (IsEqual(first.Y, second.Y))
            {
                return first.X > second.X;
            }

            return first.Y > second.Y;
        }

        /// <summary>
        /// Kiểm tra giá trị 1 có nhỏ hơn giá trị 2 hay không trong môi trường Revit
        /// True: lớn hơn, False: nhỏ hơn hoặc bằng
        /// </summary>
        /// <param name="first">Giá trị 1</param>
        /// <param name="second">Giá trị 2</param>
        /// <returns></returns>
        public static bool IsBigger(double first, double second)
        {
            if (IsEqual(first, second)) return false;
            return first > second;
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng hướng và cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsSameDirection(XYZ firstVec, XYZ secondVec)
        {
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);
            var dot = DotMatrix(first, second);
            return IsEqual(dot, 1);
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng và cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsSameDirection(UV firstVec, UV secondVec)
        {
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);
            var dot = DotMatrix(first, second);
            return (IsEqual(dot, 1));
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsSameOrOppositeDirection(XYZ firstVec, XYZ secondVec)
        {
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to 1, return true
            var dot = DotMatrix(first, second);
            return IsEqual(dot, 1) || IsEqual(dot, -1);
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng với nhau hay không trong môi trường Revit
        /// True: cùng phương và hướng, False: khác phương hoặc hướng
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsSameOrOppositeDirection(UV firstVec, UV secondVec)
        {
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to 1, return true
            var dot = DotMatrix(first, second);
            return IsEqual(dot, 1) || IsEqual(dot, -1);
        }

        /// <summary>
        /// Kiểm tra 2 vector có cùng hướng và ngược phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và ngược hướng, False: các trường hợp còn lại
        /// </summary>
        /// <param name="firstVec">Vector 1</param>
        /// <param name="secondVec">Vector 2</param>
        /// <returns></returns>
        public static bool IsOppositeDirection(XYZ firstVec, XYZ secondVec)
        {
            // get the unit vector for two vectors
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);
            // if the dot product of two unit vectors is equal to -1, return true
            var dot = DotMatrix(first, second);
            return IsEqual(dot, -1);
        }

        /// <summary>
        /// Kiểm tra 2 vector 2d có cùng hướng và ngược phương với nhau hay không trong môi trường Revit
        /// True: cùng phương và ngược hướng, False: các trường hợp còn lại
        /// </summary>
        /// <param name="firstVec">Vector 2d 1</param>
        /// <param name="secondVec">Vector 2d 2</param>
        /// <returns></returns>
        public static bool IsOppositeDirection(UV firstVec, UV secondVec)
        {
            // get the unit vector for two vectors
            var first = UnitVector(firstVec);
            var second = UnitVector(secondVec);

            // if the dot product of two unit vectors is equal to -1, return true
            var dot = DotMatrix(first, second);
            return IsEqual(dot, -1);
        }

        /// <summary>
        /// Trả về vector là tích có hướng của 2 vector cho trước
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static XYZ CrossMatrix(XYZ p1, XYZ p2)
        {
            //get the coordinate of the XYZ
            var u1 = p1.X;
            var u2 = p1.Y;
            var u3 = p1.Z;

            var v1 = p2.X;
            var v2 = p2.Y;
            var v3 = p2.Z;

            var x = v3 * u2 - v2 * u3;
            var y = v1 * u3 - v3 * u1;
            var z = v2 * u1 - v1 * u2;

            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector đơn vị của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <returns></returns>
        public static XYZ UnitVector(XYZ vector)
        {
            // calculate the distance from grid origin to the XYZ
            var length = GetLength(vector);

            // changed the vector into the unit length
            var x = vector.X / length;
            var y = vector.Y / length;
            var z = vector.Z / length;
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector đơn vị của vector 2d cho trước
        /// </summary>
        /// <param name="vector">Vector 2d cho trước</param>
        /// <returns></returns>
        public static UV UnitVector(UV vector)
        {
            // calculate the distance from grid origin to the XYZ
            var length = GetLength(vector);

            // changed the vector into the unit length
            var x = vector.U / length;
            var y = vector.V / length;
            return new UV(x, y);
        }

        /// <summary>
        /// Trả về chiều dài của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <returns></returns>
        public static double GetLength(XYZ vector)
        {
            var x = vector.X;
            var y = vector.Y;
            var z = vector.Z;
            return Math.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Trả về chiều dài đại số của vector cho trước
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <param name="checkPositive">Kiểu dữ liệu bool yêu cầu đại số vector hay không (true: có, false: không)</param>
        /// <returns></returns>
        public static double GetLength(XYZ vector, bool checkPositive)
        {
            var len = GetLength(vector);
            if (!checkPositive) return len;
            return IsSmaller(-vector, vector) ? len : -len;
        }

        /// <summary>
        /// Trả về chiều dài của vector 2d cho trước
        /// </summary>
        /// <param name="vector">Vector 2d cho trước</param>
        /// <returns></returns>
        public static double GetLength(UV vector)
        {
            var x = vector.U;
            var y = vector.V;
            return Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Trả về chiều dài của đoạn thẳng cho trước
        /// </summary>
        /// <param name="line">Đoạn thẳng cho trước</param>
        /// <returns></returns>
        public static double GetLength(Line line)
        {
            var first = line.GetEndPoint(0);
            var second = line.GetEndPoint(1);
            var vec = SubXYZ(first, second);
            return GetLength(vec);
        }

        /// <summary>
        /// Trả về khoảng cách của 2 tọa độ cho trước
        /// </summary>
        /// <param name="p1">Tọa độ 1</param>
        /// <param name="p2">Tọa độ 2</param>
        /// <returns></returns>
        public static double GetLength(XYZ p1, XYZ p2)
        {
            return GetLength(SubXYZ(p1, p2));
        }

        /// <summary>
        /// Trả về tọa độ là trung điểm của 2 tọa độ cho trước
        /// </summary>
        /// <param name="first">Tọa độ 1</param>
        /// <param name="second">Tọa độ 2</param>
        /// <returns></returns>
        public static XYZ GetMiddlePoint(XYZ first, XYZ second)
        {
            return (first + second) / 2;
        }

        /// <summary>
        /// Trả về vector đi từ điểm 2 về điểm 1
        /// </summary>
        /// <param name="p1">Điểm 1</param>
        /// <param name="p2">Điểm 2</param>
        /// <returns></returns>
        public static XYZ SubXYZ(XYZ p1, XYZ p2)
        {
            var x = p1.X - p2.X;
            var y = p1.Y - p2.Y;
            var z = p1.Z - p2.Z;

            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector 2d đi từ điểm 2 về điểm 1
        /// </summary>
        /// <param name="p1">Điểm 2d 1</param>
        /// <param name="p2">Điểm 2d 2</param>
        /// <returns></returns>
        public static UV SubXYZ(UV p1, UV p2)
        {
            var x = p1.U - p2.U;
            var y = p1.V - p2.V;

            return new UV(x, y);
        }

        /// <summary>
        /// Trả về vector là tổng 2 vector cho trước
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static XYZ AddXYZ(XYZ p1, XYZ p2)
        {
            var x = p1.X + p2.X;
            var y = p1.Y + p2.Y;
            var z = p1.Z + p2.Z;

            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Trả về vector 2d là tổng 2 vector 2d cho trước
        /// </summary>
        /// <param name="p1">Vector 2d 1</param>
        /// <param name="p2">Vector 2d 2</param>
        /// <returns></returns>
        public static UV AddXYZ(UV p1, UV p2)
        {
            var x = p1.U + p2.U;
            var y = p1.V + p2.V;

            return new UV(x, y);
        }

        /// <summary>
        /// Trả về vector là kết quả của nhân vector với một giá trị
        /// </summary>
        /// <param name="vector">Vector cho trước</param>
        /// <param name="rate">Giá trị cho trước</param>
        /// <returns></returns>
        public static XYZ MultiplyVector(XYZ vector, double rate)
        {
            var x = vector.X * rate;
            var y = vector.Y * rate;
            var z = vector.Z * rate;

            return new XYZ(x, y, z);
        }

        public static XYZ TransformPoint(XYZ point, Transform transform)
        {
            //get the coordinate value in X, Y, Z axis
            var x = point.X;
            var y = point.Y;
            var z = point.Z;

            //transform basis of the old coordinate system in the new coordinate system
            var b0 = transform.get_Basis(0);
            var b1 = transform.get_Basis(1);
            var b2 = transform.get_Basis(2);
            var origin = transform.Origin;

            //transform the origin of the old coordinate system in the new coordinate system
            var xTemp = x * b0.X + y * b1.X + z * b2.X + origin.X;
            var yTemp = x * b0.Y + y * b1.Y + z * b2.Y + origin.Y;
            var zTemp = x * b0.Z + y * b1.Z + z * b2.Z + origin.Z;

            return new XYZ(xTemp, yTemp, zTemp);
        }

        public static Curve TransformCurve(Curve curve, Transform transform)
        {
            return Line.CreateBound(TransformPoint(curve.GetEndPoint(0), transform), TransformPoint(curve.GetEndPoint(1), transform));
        }

        /// <summary>
        /// Trả về một điểm là kết quả của tịnh tiến một điểm theo một vector và khoảng cách trước
        /// </summary>
        /// <param name="point">Điểm cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static XYZ OffsetPoint(XYZ point, XYZ direction, double offset)
        {
            var directUnit = UnitVector(direction);
            var offsetVect = MultiplyVector(directUnit, offset);
            return AddXYZ(point, offsetVect);
        }

        /// <summary>
        /// Trả về một đoạn thẳng là kết quả của tịnh tiến một đoạn thẳng theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="c">Đoạn thẳng cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static Curve OffsetCurve(Curve c, XYZ direction, double offset)
        {
            c = Line.CreateBound(OffsetPoint(c.GetEndPoint(0), direction, offset), OffsetPoint(c.GetEndPoint(1), direction, offset));
            return c;
        }

        /// <summary>
        /// Trả về một tập hợp các đoạn thẳng là kết quả của tịnh tiến tập hợp đoạn thẳng theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="cs">Tập hợp các đoạn thẳng cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static List<Curve> OffsetListCurve(List<Curve> cs, Autodesk.Revit.DB.XYZ direction, double offset)
        {
            for (var i = 0; i < cs.Count; i++)
            {
                cs[i] = OffsetCurve(cs[i], direction, offset);
            }

            return cs;
        }

        /// <summary>
        /// Trả về một polygon là kết quả của tịnh tiến một polygon theo một vector và khoảng cách cho trước
        /// </summary>
        /// <param name="pl">Polygon cho trước</param>
        /// <param name="direction">Vector cho trước</param>
        /// <param name="offset">Khoảng cách cho trước</param>
        /// <returns></returns>
        public static Polygon OffsetPolygon(Polygon pl, XYZ direction, double offset)
        {
            var cs = (from c in pl.ListCurve select OffsetCurve(c, direction, offset)).ToList();
            return new Polygon(cs);
        }

        /// <summary>
        /// Trả về giá trị là tích vô hướng của 2 vector
        /// </summary>
        /// <param name="p1">Vector 1</param>
        /// <param name="p2">Vector 2</param>
        /// <returns></returns>
        public static double DotMatrix(XYZ p1, XYZ p2)
        {
            //get the coordinate of the Autodesk.Revit.DB.XYZ
            var v1 = p1.X;
            var v2 = p1.Y;
            var v3 = p1.Z;

            var u1 = p2.X;
            var u2 = p2.Y;
            var u3 = p2.Z;

            // tich vô hướng vuong goc thi = 0
            return v1 * u1 + v2 * u2 + v3 * u3;
        }

        /// <summary>
        /// Trả về giá trị là tích vô hướng của 2 vector 2d
        /// </summary>
        /// <param name="p1">Vector 2d 1</param>
        /// <param name="p2">Vector 2d 2</param>
        /// <returns></returns>
        public static double DotMatrix(UV p1, UV p2)
        {
            //get the coordinate of the Autodesk.Revit.DB.XYZ
            var v1 = p1.U;
            var v2 = p1.V;

            var u1 = p2.U;
            var u2 = p2.V;

            return v1 * u1 + v2 * u2;
        }

        /// <summary>
        /// Hàm làm tròn lên giá trị và ép về lại kiểu int
        /// </summary>
        /// <param name="d">Giá trị đang xét</param>
        /// <returns></returns>
        public static int RoundUp(double d)
        {
            return Math.Round(d, 0) < d ? (int)(Math.Round(d, 0) + 1) : (int)(Math.Round(d, 0));
        }

        /// <summary>
        /// Hàm làm tròn xuống giá trị và ép về lại kiểu int
        /// </summary>
        /// <param name="d">Giá trị đang xét</param>
        /// <returns></returns>
        public static int RoundDown(double d)
        {
            return Math.Round(d, 0) < d ? (int)(Math.Round(d, 0)) : (int)(Math.Round(d, 0) - 1);
        }

        /// <summary>
        /// Trả về góc radian của 2 vector đang xét
        /// </summary>
        /// <param name="vec1">Vector 1</param>
        /// <param name="vec2">Vector 2</param>
        /// <returns></returns>
        public static double GetRadianAngle(XYZ vec1, XYZ vec2)
        {
            return Math.Acos(DotMatrix(UnitVector(vec1), UnitVector(vec2)));
        }

        /// <summary>
        /// Trả về góc degree của 2 vector đang xét
        /// </summary>
        /// <param name="vec1">Vector 1</param>
        /// <param name="vec2">Vector 2</param>
        /// <returns></returns>
        public static double GetDegreeAngle(XYZ vec1, XYZ vec2)
        {
            return Radian2Degree(GetRadianAngle(vec1, vec2));
        }

        public static double GetAngle(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            vecX = vecX.Normalize();

            vecY = vecY.Normalize();

            var tm = vecX.DotProduct(vecY);

            var T = vecX.CrossProduct(vecY);

            var Tp = T.DotProduct(targetVec);

            //khac
            if (!IsEqual(tm, 0)) throw new Exception("Two basis is not perpecular!");

            // khac
            if (!IsEqual(Tp, 0))
            {
                throw new Exception("TargetVector is not planar with two basis!");
            }

            double angle = 0;

            var angle1 = vecX.AngleTo(targetVec);

            var angle2 = Math.PI - angle1;

            var cos = Math.Cos(angle1);

            var sin = Math.Sin(angle1);

            // vecto chiếu lại
            var assVec = vecX * cos + vecY * sin;

            // nếu cùng hướng và cùng phương
            if (IsSameDirection(assVec, targetVec))
            {
                angle = angle1;
            }

            // nếu cùng phương và ngược hướng
            else if (IsOppositeDirection(assVec, targetVec))
            {
                angle = -angle2;

                // c2
                //angle = Math.PI/2 + angle2
            }

            // không cùng phương và không cùng hướng (bù)
            else
            {
                assVec = vecX * Math.Cos(angle2) + vecY * Math.Sin(angle2);
                // cùng phương và cùng hướng
                if (IsSameDirection(assVec, targetVec))
                {
                    angle = angle2;
                }
                // cung phương và ngược hướng
                else if (IsOppositeDirection(assVec, targetVec))
                {
                    angle = -angle1;

                    // c2
                    //angle = Math.PI/2 + angle1
                }
                else throw new Exception("The code should never go here!");
            }

            return angle;
        }

        public static UV GetUvCoordinate(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            vecX = vecX.Normalize();
            vecY = vecY.Normalize();
            var len = targetVec.GetLength();
            var angle = GetAngle(targetVec, vecX, vecY);
            return new UV(Math.Cos(angle) * len, Math.Sin(angle) * len);
        }

        public static double GetAngle2(XYZ targetVec, XYZ vecX, XYZ vecY, XYZ vecZ)
        {
            vecX = vecX.Normalize();
            vecY = vecY.Normalize();
            vecZ = vecZ.Normalize();

            if (!IsEqual(vecX.DotProduct(vecY), 0)) throw new Exception("Two basis X and Y is not perpecular!");

            if (!IsEqual(vecX.CrossProduct(vecY), vecZ)) throw new Exception("Three basises are not a coordiante!");

            var pl = Plane.CreateByOriginAndBasis(XYZ.Zero, vecX, vecY);

            var pjPnt = CheckGeometry.GetProjectPoint(pl, targetVec);

            return GetAngle(targetVec, pjPnt, vecZ);
        }

        public static XYZ GetPositionVector(XYZ targetVec, XYZ vecX, XYZ vecY)
        {
            XYZ vec = null;
            var ang = GetAngle(targetVec, vecX, vecY);
            if (IsEqual(Math.Abs(ang), Math.PI / 2) || ang < Math.PI / 2)
            {
                vec = targetVec;
            }
            else if (ang > Math.PI / 2)
            {
                vec = -targetVec;
            }

            return vec;
        }
    }

    public class ZyxComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            // first compare z coordinate, then y coordiante, at last x coordinate
            if (second != null && (first == null || !GeometryUtils.IsEqual(first.Z, second.Z))) return first != null && first.Z > second.Z ? 1 : -1;
            if (second != null && !GeometryUtils.IsEqual(first.Y, second.Y)) return first.Y > second.Y ? 1 : -1;
            if (second != null && GeometryUtils.IsEqual(first.X, second.X))
            {
                return 0; // Equal
            }

            return second != null && first.X > second.X ? 1 : -1;
        }
    }

    public class XYComparer : IComparer<XYZ>
    {
        int IComparer<XYZ>.Compare(XYZ first, XYZ second)
        {
            if (!GeometryUtils.IsEqual(first.X, second.X)) return first.X > second.X ? 1 : -1;
            if (GeometryUtils.IsEqual(first.Y, second.Y))
            {
                return 0; // Equal
            }

            return first.Y > second.Y ? 1 : -1;
        }
    }

    /// <summary>
    /// Hàm sắp xếp các đối tượng trong Revit dựa trên tọa độ điểm đặt các đối tượng
    /// Các tọa độ được sắp xếp theo thứ tự X -> Y
    /// </summary>
    public class ElementLocationComparer : IComparer<Element>
    {
        private readonly Plane Plane;

        public ElementLocationComparer()
        {
        }

        public ElementLocationComparer(Plane pl)
        {
            Plane = pl;
        }

        int IComparer<Element>.Compare(Element x, Element y)
        {
            XYZ loc1 = null;
            XYZ loc2 = null;
            switch (x)
            {
                case Wall wall:
                    {
                        var wgi = new WallGeometryInfo(wall);
                        loc1 = wgi.TopPolygon.CentralXYZPoint;
                        break;
                    }
                case FamilyInstance instance:
                    {
                        var cgi = new ColumnGeometryInfo(instance);
                        loc1 = cgi.TopPolygon.CentralXYZPoint;
                        break;
                    }
            }

            switch (y)
            {
                case Wall wall1:
                    {
                        var wgi = new WallGeometryInfo(wall1);
                        loc2 = wgi.TopPolygon.CentralXYZPoint;
                        break;
                    }
                case FamilyInstance instance:
                    {
                        var cgi = new ColumnGeometryInfo(instance);
                        loc2 = cgi.TopPolygon.CentralXYZPoint;
                        break;
                    }
            }

            loc1 = new XYZ(loc1.X, loc1.Y, 0);
            loc2 = new XYZ(loc2.X, loc2.Y, 0);
            if (Plane != null)
            {
                loc1 = CheckGeometry.GetProjectPoint(Plane, loc1);
                loc2 = CheckGeometry.GetProjectPoint(Plane, loc2);
            }

            if (!GeometryUtils.IsEqual(loc1.X, loc2.X)) return loc1.X > loc2.X ? 1 : -1;
            if (GeometryUtils.IsEqual(loc1.Y, loc2.Y))
            {
                return 0; // Equal
            }

            return (loc1.Y > loc2.Y) ? 1 : -1;
        }
    }

    public class Angle
    {
        public double Radian { get; set; }
        public double Degree { get; set; }
        public XYZ TargetVector { get; set; }

        public Angle(XYZ targetVec, XYZ baseVec)
        {
            TargetVector = targetVec;
            Radian = TargetVector.AngleTo(baseVec);
            Degree = Radian * 180 / Math.PI;
        }

        public Angle(XYZ targetVec, XYZ xVec, XYZ yVec)
        {
            TargetVector = targetVec;
            Radian = TargetVector.AngleTo(xVec);
            var assTargetVec = xVec * Math.Cos(Radian) + yVec * Math.Sin(Radian);
            if (GeometryUtils.IsSameDirection(assTargetVec, TargetVector))
            {
            }
            else if (GeometryUtils.IsOppositeDirection(assTargetVec, TargetVector))
            {
                Radian *= -1;
            }
            else
            {
                Radian = Math.PI - Radian;
                assTargetVec = xVec * Math.Cos(Radian) + yVec * Math.Sin(Radian);
                if (GeometryUtils.IsSameDirection(assTargetVec, TargetVector))
                {
                }
                else if (GeometryUtils.IsOppositeDirection(assTargetVec, TargetVector))
                {
                    Radian *= -1;
                }
                else
                {
                    throw new Exception("Wrong Calculation!");
                }
            }

            Degree = Radian * 180 / Math.PI;
        }
    }

    public enum DirectShapeTypeEnum
    {
        PLASTER_BEAM_BOTTOM,
        PLASTER_BEAM_SIDES,
        PLASTER_COLUMN,
        PLASTER_WALL,
        PLASTER_FLOOR,
        FORMWORK_BEAM_BOTTOM,
        FORMWORK_BEAM_SIDES,
        FORMWORK_COLUMN,
        FORMWORK_FLOOR,
        FORMWORK_WALL,
        FORMWORK_FOUNDATION,
        FORMWORK_LININGFOUNDATION,
        FORMWORK_LININGGROUNDBEAM,
        FORMWORK_LININGGROUNDSLAB,
        FORMWORK_LINING
    }
}