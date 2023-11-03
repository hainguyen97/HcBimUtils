namespace HcBimUtils.JsonData.Ribbons
{
    public class CustomRibbon
    {
        public CustomRibbon()
        {
        }

        public string Name { get; set; }

        public List<CustomRibbonPanel> Panels { get; set; } = new();
    }
}