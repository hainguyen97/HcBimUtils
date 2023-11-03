namespace HcBimUtils.JsonData.Rebars
{
    public class RebarLocationJson
    {
        public bool IsFilterRebarTopBottom { get; set; } = true;
        public bool IsFilterRebarOxOy { get; set; } = true;
        public string TopParameter { get; set; }
        public string BotParameter { get; set; }
        public string OxParameter { get; set; }
        public string OyParameter { get; set; }
        public string TopValue { get; set; } = "Top";
        public string BotValue { get; set; } = "Bot";
        public string OxValue { get; set; } = "Ox";
        public string OyValue { get; set; } = "Oy";

        public RebarLocationJson()
        {
        }
    }
}