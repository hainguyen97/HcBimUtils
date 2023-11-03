namespace HcBimUtils.JsonData.MepTools.AlignAngle
{
    public class AlignAngleJson
    {
        public string AlignmentType { get; set; } = "Top Left";
        public bool IsFreeEnd { get; set; }
        public bool IsFixedLanding { get; set; }
        public bool IsIntermittentAlignment { get; set; }
        public int Degree { get; set; }
        public double VerticalSpacing { get; set; } = 1000.MmToFoot();
        public double HorizontalLanding { get; set; } = 1000.MmToFoot();
        public double HorizontalSpacing { get; set; } = 1000.MmToFoot();

        public AlignAngleJson()
        {
        }
    }
}