namespace HcBimUtils.JsonData.Geometry.Coordinate
{
    public class JLine
    {
        public JXYZ Start { get; set; }
        public JXYZ EndPoint { get; set; }
        public JXYZ Direction { get; set; }
        public double Length { get; set; }

        public JLine()
        {
        }
    }
}