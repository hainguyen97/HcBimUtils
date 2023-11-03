namespace HcBimUtils.JsonData.ShearWallRebar
{
    public class CrossSectionSettingJson
    {
        public bool IsCreate { get; set; }

        public string StirrupTag { get; set; }
        public string StandardTag { get; set; }

        public string DimensionType { get; set; }
        public string ViewTemplate { get; set; }
        public int Scale { get; set; } = 25;
        public string ViewFamilyType { get; set; }

        public double Position { get; set; } = 0.5;

        public CrossSectionSettingJson()
        {
        }
    }
}