

namespace HcBimUtils.JsonData.Slab
{
    public class SlabSettingJson
    {
        public string TagName { get; set; }
        public double L1 { get; set; } = 150.MmToFoot();
        public double L2 { get; set; } = 150.MmToFoot();
        public bool AssignLength { get; set; } = true;
        public bool DrawSymbol { get; set; } = true;
        public bool DrawKhoangRai { get; set; } = true;
        public int KieuBoTriThep { get; set; } = 1;
        public bool IsAbove { get; set; } = true;
        public int Diameter { get; set; } = 10;
        public double Spacing { get; set; } = 150.MmToFoot();
        public double Cover { get; set; } = 30.MmToFoot();
        public int Direction { get; set; } = 1;

        public SlabSettingJson()
        {
        }
    }
}