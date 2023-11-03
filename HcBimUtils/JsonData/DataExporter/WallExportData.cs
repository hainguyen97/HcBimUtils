using HcBimUtils.JsonData.Geometry.Coordinate;

namespace HcBimUtils.JsonData.DataExporter
{
    public class WallExportData
    {
        public List<JRectangle> JRectangles { get; set; } = new List<JRectangle>();
        public List<JTriangle> JTriangles { get; set; } = new List<JTriangle>();
        public List<JWallRebar> JWallRebars { get; set; } = new List<JWallRebar>();

        public WallExportData()
        {
        }
    }
}