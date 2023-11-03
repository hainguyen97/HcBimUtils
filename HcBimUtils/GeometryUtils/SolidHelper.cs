using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils
{
    public static class SolidHelper
    {
        public static Solid CreateSolidFromBoundingBoxOfSolid(this Solid inputSolid)
        {
            var bbox = inputSolid.GetBoundingBox();

            // Corners in BBox coords

            var pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            var pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            var pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            var pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            // Edges in BBox coords

            var edge0 = Line.CreateBound(pt0, pt1);
            var edge1 = Line.CreateBound(pt1, pt2);
            var edge2 = Line.CreateBound(pt2, pt3);
            var edge3 = Line.CreateBound(pt3, pt0);

            // Create loop, still in BBox coords

            var edges = new List<Curve> { edge0, edge1, edge2, edge3 };

            var height = bbox.Max.Z - bbox.Min.Z;

            var baseLoop = CurveLoop.Create(edges);

            var loopList = new List<CurveLoop> { baseLoop };

            var preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);

            var transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);

            return transformBox;
        }

        public static Solid CreateSolidFromBoundingBox(this BoundingBoxXYZ bbox)
        {
            // Corners in BBox coords

            var pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            var pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            var pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            var pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            // Edges in BBox coords

            var edge0 = Line.CreateBound(pt0, pt1);
            var edge1 = Line.CreateBound(pt1, pt2);
            var edge2 = Line.CreateBound(pt2, pt3);
            var edge3 = Line.CreateBound(pt3, pt0);

            // Create loop, still in BBox coords

            var edges = new List<Curve> { edge0, edge1, edge2, edge3 };

            var height = bbox.Max.Z - bbox.Min.Z;

            var baseLoop = CurveLoop.Create(edges);

            var loopList = new List<CurveLoop> { baseLoop };

            var preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);

            var transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);

            return transformBox;
        }
    }
}