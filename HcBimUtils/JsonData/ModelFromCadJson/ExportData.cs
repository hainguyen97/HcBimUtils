namespace HcBimUtils.JsonData.ModelFromCadJson
{
    public class ExportData
    {
        public Dictionary<string, ExportPileData> DicPileData { get; set; } = new();

        public Dictionary<string, ExportColumnData> DicColumnData { get; set; } = new();

        public Dictionary<string, ExportBeamData> DicBeamData { get; set; } = new();

        public Dictionary<string, FloorData> DicFloorData { get; set; } = new();

        public Dictionary<string, FloorData> DicFoundationData { get; set; } = new();

        public Dictionary<string, FloorData> DicWallData { get; set; } = new();
        public Dictionary<string, ExportGridData> DicGridData { get; set; } = new();

        public Dictionary<string, ExportLevelData> DicLevelData { get; set; } = new();
        public Dictionary<string, ExportBlockData> DicBlockData { get; set; } = new();

        public ExportData()
        {
        }
    }
}