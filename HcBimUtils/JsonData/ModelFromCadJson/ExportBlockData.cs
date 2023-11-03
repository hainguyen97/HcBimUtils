
namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportBlockData
    {
        public List<BlockData> BlockDatas { get; set; } = new List<BlockData>();
        public XyzData Origin { get; set; }

        public ExportBlockData()
        {
        }
    }
}