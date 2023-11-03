namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportBeamData
    {
        public List<LineData> LineDatas { get; set; }
        public List<TextData> TextDatas { get; set; }
        public XyzData Origin { get; set; }
    }
}