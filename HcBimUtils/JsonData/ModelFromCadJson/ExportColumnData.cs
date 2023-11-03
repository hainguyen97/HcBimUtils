

namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportColumnData
    {
        public List<List<XyzData>> ListPoint { get; set; } = new();
        public XyzData Origin { get; set; }

        public ExportColumnData()
        {
        }
    }
}