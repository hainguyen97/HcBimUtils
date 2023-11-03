namespace HcBimUtils.JsonData.Geometry.Coordinate
{
    public class JXYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public JXYZ()
        {
        }

        public JXYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}