namespace HcBimUtils.JsonData.Beam
{
    public class ThepCauTaoGiuaDamModel
    {
        public double MinBeamHeight { get; set; } = 700.MmToFoot();
        public int BarDiameter { get; set; } = 14;
        public double Distance { get; set; } = 400.MmToFoot();
        public double LengthGoInColumn { get; set; } = 100.MmToFoot();
        public int BarDiameterForBarGoInColumn { get; set; } = 8;
        public double DistanceForBarGoInColumn { get; set; } = 100.MmToFoot();
    }
}