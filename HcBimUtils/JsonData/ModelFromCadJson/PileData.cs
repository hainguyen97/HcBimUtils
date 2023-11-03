namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class PileData
    {
        public XyzData Location { get; set; }
        public double Diameter { get; set; }
        public List<XyzData> ListPoint { get; set; } = new();

        public PileData()
        {
        }
    }
}