using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils
{
    public class RevitGeometryUtils
    {
        internal static List<Face> GetFacesFromSolid(Solid solid, ElementFacesToUse facesToUse)
        {
            return facesToUse switch
            {
                ElementFacesToUse.Top => GetTopFacesFromSolid(solid),
                ElementFacesToUse.Bottom => GetBottomFacesFromSolid(solid),
                ElementFacesToUse.Side => GetSideFacesFromSolid(solid),
                _ => GetAllFacesFromSolid(solid)
            };
        }

        internal static List<Solid> GetElementSolids(GeometryElement geometryElement)
        {
            var list = new List<Solid>();
            foreach (var geometryObject in geometryElement)
            {
                if (geometryObject is Solid solid)
                {
                    list.Add(solid);
                }
                else
                {
                    var geometryInstance = geometryObject as GeometryInstance;
                    if (geometryInstance == null) continue;
                    foreach (var geometryObject2 in geometryInstance.GetInstanceGeometry())
                    {
                        if (geometryObject2 is Solid object2)
                        {
                            list.Add(object2);
                        }
                    }
                }
            }

            return list;
        }

        internal static List<Solid> GetElementSolids(Element element)
        {
            var options = new Options { ComputeReferences = true, DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = true };
            return GetElementSolids(element.get_Geometry(options));
        }

        internal static List<Edge> GetElementEdges(Element element)
        {
            var list = new List<Edge>();
            foreach (var solid in GetElementSolids(element))
            {
                list.AddRange(solid.Edges.Cast<Edge>());
            }

            return list;
        }

        private static List<Face> GetTopFacesFromSolid(Solid solid)
        {
            return (from object obj in solid.Faces select ((Face)obj) as PlanarFace into planarFace where null != planarFace && planarFace.FaceNormal.IsVertical() && planarFace.FaceNormal.Z.IsGreater(0.0) select planarFace).Cast<Face>().ToList();
        }

        private static List<Face> GetSideFacesFromSolid(Solid solid)
        {
            return GetAllFacesFromSolid(solid).Except(GetTopFacesFromSolid(solid)).ToList().Except(GetBottomFacesFromSolid(solid)).ToList();
        }

        private static List<Face> GetAllFacesFromSolid(Solid solid)
        {
            return (from Face face in solid.Faces where face.Area != 0.0 select face).ToList();
        }

        private static List<Face> GetBottomFacesFromSolid(Solid solid)
        {
            return (from object obj in solid.Faces select (Face)obj as PlanarFace into planarFace where null != planarFace && planarFace.FaceNormal.IsVertical() && planarFace.FaceNormal.Z.IsSmaller(0.0) select planarFace).Cast<Face>().ToList();
        }

        [Flags]
        public enum ElementFacesToUse
        {
            Top = 1,
            Bottom = 2,
            Side = 4,
            All = 7
        }
    }
}