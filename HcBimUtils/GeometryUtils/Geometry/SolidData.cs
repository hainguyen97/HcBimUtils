using Autodesk.Revit.DB;

namespace HcBimUtils.GeometryUtils.Geometry
{
    public class SolidData
    {
        public Solid Solid { get; }

        public GeometryInstance GeometryInstance { get; }

        public SolidData(Solid solid, GeometryInstance geometryInstance)
        {
            Solid = solid;
            GeometryInstance = geometryInstance;
        }
    }
}