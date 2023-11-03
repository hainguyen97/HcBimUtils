namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportPileData
    {
        public List<PileData> PileDatas { get; set; } = new();
        public List<TextData> Marks { get; set; } = new();
        public XyzData Origin { get; set; }

        public ExportPileData()
        {
        }
    }
}