using Autodesk.Revit.DB;
using HcBimUtils.DocumentUtils;
using HcBimUtils.GeometryUtils;

namespace HcBimUtils
{
    public static class FaceUtils
    {
        public static CurveLoop GetFirstCurveLoop(this Face face)
        {
            return face.GetEdgesAsCurveLoops().OrderByDescending(x => x.GetExactLength()).FirstOrDefault();
        }




        public static List<XYZ> GetPoints(this Face face)
        {
            List<XYZ> points = new List<XYZ>();
            var curveLoop = face.GetEdgesAsCurveLoops().OrderByDescending(x => x.GetExactLength()).FirstOrDefault();

            foreach (Curve curve in curveLoop)
            {
                points.Add(curve.GetEndPoint(0));
            }

            return points;
        }
        // Có xét đến giá trị âm / dương
        public static double DistanceNormalizeTo(this Face face, XYZ point, XYZ normalize)
        {
#if Version2017 || Version2018
         var result = face.Project(point);
         double distance = 0;

         if (result != null)
         {
            distance = result.Distance;
         }

         var intersect = face.Project(point);

         if (intersect != null)
         {
            XYZ projectPoint = intersect.XYZPoint;

            XYZ directionPoint = (point - projectPoint).Normalize();

            if (directionPoint.IsCodirectionalTo(normalize))
            {
               return distance;
            }
            else
            {
               return -distance;
            }
         }

         return distance;
#else
            if (face != null && point != null)
            {
                UV uv = UV.Zero;
                double distance = 0;
                face.GetSurface().Project(point, out uv, out distance);

                var intersect = face.Project(point);

                if (intersect != null)
                {
                    XYZ projectPoint = intersect.XYZPoint;

                    XYZ directionPoint = (point - projectPoint).Normalize();

                    if (directionPoint.IsCodirectionalTo(normalize))
                    {
                        return distance;
                    }
                    else
                    {
                        return -distance;
                    }
                }

                return distance;
            }
#endif

            return 0;
        }
        public static double DistanceTo(this Face face, XYZ point)
        {
#if Version2017 || Version2018
         var result = face.Project(point);
         if (result != null)
         {
            return result.Distance;
         }
#else
            if (face != null && point != null)
            {
                UV uv = UV.Zero;
                double distance = 0;
                face.GetSurface().Project(point, out uv, out distance);
                return distance;
            }
#endif

            return 0;
        }
        public static bool IsHorizontalFaceUp(this Face face)
        {
            var rs = false;
            var normal = face.ComputeNormal(UV.Zero);
            if (normal.IsVerticalUp())
            {
                rs = true;
            }
            return rs;
        }

        public static bool IsHorizontalFaceDown(this Face face)
        {
            var rs = false;
            var normal = face.ComputeNormal(UV.Zero);
            if (normal.IsVerticalDown())
            {
                rs = true;
            }
            return rs;
        }

        public static bool IsHorizontalFace(this Face face)
        {
            var rs = false;
            var normal = face.ComputeNormal(UV.Zero);
            if (normal.IsVertical())
            {
                rs = true;
            }
            return rs;
        }

        public static bool IsVerticalFace(this Face face)
        {
            var rs = false;
            var normal = face.ComputeNormal(UV.Zero);
            if (normal.IsHorizontal())
            {
                rs = true;
            }
            return rs;
        }

        public static XYZ FaceCenter(this Face face)
        {
            var mesh = face.Triangulate();
            var vertices = mesh.Vertices;
            var center = XYZ.Zero;
            foreach (var vertex in vertices)
            {
                center = center + vertex;
            }
            return center / vertices.Count;
        }

        public static Solid SolidByFace(this Face face, XYZ direction, double height, bool offset = false)
        {
            IList<CurveLoop> listCurveLoop = new List<CurveLoop>();
            if (face is PlanarFace)
            {
                foreach (var cl in face.GetEdgesAsCurveLoops())
                {
                    var newcl = cl;
                    if (offset)
                    {
                        var count = 0;
                        foreach (var curve in cl)
                        {
                            count++;
                        }
                        if (count == 4)
                        {
                            newcl = CurveLoop.CreateViaOffset(cl, height, cl.GetPlane().Normal);
                            if (newcl.PerimeterCurveLoop() < cl.PerimeterCurveLoop())
                            {
                                newcl = CurveLoop.CreateViaOffset(cl, -height, cl.GetPlane().Normal);
                            }
                        }
                    }

                    //listCurveLoop.Add(newcl.FixCurveLoop());
                    listCurveLoop.Add(cl);
                }
                Solid s = null;
                try
                {
                    s = GeometryCreationUtilities.CreateExtrusionGeometry(listCurveLoop, direction, height);
                }
                catch
                {
                }
                return s;
            }

            return null;
        }

        public static bool IsOverlap(Face face1, Face face2)
        {
            if (face1 != null && face1 != null)
            {
                if (face1.Intersect(face2) == FaceIntersectionFaceResult.NonIntersecting)
                {
                    foreach (EdgeArray edgeArray in face1.EdgeLoops)
                    {
                        foreach (Edge edge in edgeArray)
                        {
                            Curve curve = edge.AsCurve();
                            XYZ startPoint = curve.GetEndPoint(0);
                            XYZ endPoint = curve.GetEndPoint(1);

                            if (!face2.Project(startPoint).Distance.IsZero())
                            {
                                return false;
                            }

                            if (!face2.Project(endPoint).Distance.IsZero())
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public static bool IsParallel(Face face1, Face face2)
        {
            if (face1 != null && face1 != null)
            {
                if (face1.Intersect(face2) == FaceIntersectionFaceResult.NonIntersecting)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsIntersecting(Face face1, Face face2)
        {
            if (face1 != null && face1 != null)
            {
                if (face1.Intersect(face2) == FaceIntersectionFaceResult.Intersecting)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Face> AllFacesFromElement(this Element familyInstance)
        {
            var faces = new List<Face>();
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            //op.DetailLevel = ViewDetailLevel.Undefined;
            var doc = familyInstance.Document;
            op.View = doc.ActiveView;
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                foreach (Face f in solid.Faces)
                {
                    faces.Add(f);
                }
            }
            if (faces.Count < 1)
            {
                foreach (var geoO in geoE)
                {
                    var geoI = geoO as GeometryInstance;
                    if (geoI == null) continue;
                    var instanceGeoE = geoI.GetSymbolGeometry();
                    var tf = geoI.Transform;
                    foreach (var instanceGeoObj in instanceGeoE)
                    {
                        var solid1 = instanceGeoObj as Solid;
                        var solid = SolidUtils.CreateTransformed(solid1, tf);
                        if (solid == null || solid.Faces.Size == 0) continue;
                        foreach (Face face in solid.Faces)
                        {
                            faces.Add(face);
                        }
                    }
                }
            }
            return faces;
        }

        public static List<Face> AllFacesFromFamilyInstance(this FamilyInstance familyInstance)
        {
            var faces = new List<Face>();
            var op = new Options();
            op.ComputeReferences = true;
            op.IncludeNonVisibleObjects = true;
            op.DetailLevel = ViewDetailLevel.Undefined;
            var geoE = familyInstance.get_Geometry(op);
            if (geoE == null) return faces;
            foreach (var geoO in geoE)
            {
                var solid = geoO as Solid;
                if (solid == null || solid.Faces.Size == 0 || solid.Edges.Size == 0) continue;
                foreach (Face f in solid.Faces)
                {
                    faces.Add(f);
                }
            }
            if (faces.Count < 1)
            {
                foreach (var geoO in geoE)
                {
                    var geoI = geoO as GeometryInstance;
                    if (geoI == null) continue;
                    var instanceGeoE = geoI.GetSymbolGeometry();
                    var tf = geoI.Transform;
                    foreach (var instanceGeoObj in instanceGeoE)
                    {
                        var solid1 = instanceGeoObj as Solid;
                        var solid = SolidUtils.CreateTransformed(solid1, tf);
                        if (solid == null || solid.Faces.Size == 0) continue;
                        foreach (Face face in solid.Faces)
                        {
                            faces.Add(face);
                        }
                    }
                }
            }
            return faces;
        }
    }

    public class FaceAndSolid
    {
        public Face Face { get; set; }

        public Solid Solid { get; set; }
        public string Material { get; set; }

        public bool IsLining { get; set; } = false;

        public FaceAndSolid(Face faceSolid, Solid solidElement)
        {
            Face = faceSolid;
            try
            {
                if (faceSolid.MaterialElementId != null)
                {
                    Material = faceSolid.MaterialElementId.ToElement()?.Name ?? "";
                    if (Material.Contains("04") && Material.ToUpper().Contains("LC"))
                    {
                        IsLining = true;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }


            Solid = solidElement;
        }
    }
}