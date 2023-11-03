namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class XyzData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public XyzData(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public XyzData Mid(XyzData other)
        {
            var a = (X + other.X) / 2;
            var b = (Y + other.Y) / 2;
            var c = (Z + other.Z) / 2;
            return new XyzData(a, b, c);
        }
    }
}