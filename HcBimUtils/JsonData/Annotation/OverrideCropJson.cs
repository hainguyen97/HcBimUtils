namespace HcBimUtils.JsonData.Annotation
{
    public class OverrideCropJson
    {
        public bool IsVisible { get; set; } = true;
        public bool IsHalfTone { get; set; } = false;
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public bool IsColorValid { get; set; } = false;
        public string LinePattern { get; set; }
        public string Weight { get; set; }
        public int Range { get; set; } = 1;

        public OverrideCropJson()
        {
        }
    }
}