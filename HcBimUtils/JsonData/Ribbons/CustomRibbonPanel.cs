namespace HcBimUtils.JsonData.Ribbons
{
    public class CustomRibbonPanel
    {
        public CustomRibbonPanel()
        {
        }

        public string Name { get; set; }

        public List<CustomButton> Buttons { get; set; } = new();
    }
}