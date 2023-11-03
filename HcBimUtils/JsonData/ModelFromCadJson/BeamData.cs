namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class BeamData
    {
        public TextData TextData { get; set; } = new TextData();
        public XyzData StartPoint { get; set; }
        public XyzData EndPoint { get; set; }
        public XyzData Origin { get; set; }

        public BeamData()
        {
        }
    }
}