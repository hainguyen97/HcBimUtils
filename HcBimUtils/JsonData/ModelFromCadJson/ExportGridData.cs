namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportGridData
    {
        public XyzData Location { get; set; }
        public List<LineData> LineDatas { get; set; } = new List<LineData>();
        public List<TextData> TextDatas { get; set; } = new List<TextData>();

        public ExportGridData()
        {
        }
    }
}