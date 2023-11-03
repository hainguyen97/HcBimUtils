

namespace HcBimUtils.JsonData.Ribbons
{
    public class CustomButton
    {
        public ButtonType ButtonType { get; set; }
        public CustomButtonData ButtonData { get; set; }
        public List<CustomButtonData> PushButtonDataCustoms { get; set; } = new();
        public CustomButtonData ButtonData1 { get; set; }
        public CustomButtonData ButtonData2 { get; set; }
        public CustomButtonData ButtonData3 { get; set; }
        public List<CustomButtonData> List1 { get; set; } = new();
        public List<CustomButtonData> List2 { get; set; } = new();
        public List<CustomButtonData> List3 { get; set; } = new();

        public CustomButton()
        {
        }
    }

    public enum ButtonType
    {
        PushButton,
        Split,
        DropDown,
        TwoStackedItems,
        ThreeStackedItems,
        TwoStackedSplitItems,
        ThreeStackedSplitItems,
        Separator
    }
}