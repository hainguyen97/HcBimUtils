namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class FloorData
    {
        public XyzData Origin { get; set; }
        public List<List<XyzData>> ListPointList { get; set; }

        public FloorData()
        {
        }
    }
}