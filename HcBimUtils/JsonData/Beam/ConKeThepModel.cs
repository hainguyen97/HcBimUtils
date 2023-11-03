namespace HcBimUtils.JsonData.Beam
{
    public class ConKeThepModel
    {
        public bool NeedConKeThep { get; set; } = true;
        public bool IsConKeBangCotThep { get; set; }

        public DiameterAndSpacingModel ConKeThepInfo { get; set; } = new DiameterAndSpacingModel()
        {
            Diameter = 25,
            Spacing = 2000.MmToFoot()
        };

        public DiameterAndSpacingModel ConKeDaiMocInfo { get; set; } = new DiameterAndSpacingModel()
        {
            Diameter = 8,
            Spacing = 2000.MmToFoot()
        };

        public ConKeThepModel()
        {
        }

        private void Initial()
        {
            //Load Data FromSetting
        }
    }
}