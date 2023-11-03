namespace HcBimUtils.JsonData.Numbering
{
    public class NumberingJson
    {
        public int Range { get; set; }
        public int Digits { get; set; }
        public int Start { get; set; }
        public int Step { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int IntProcessingOrder { get; set; }
        public double ElementPositionTolerance { get; set; }
        public string Category { get; set; }
        public string Parameter { get; set; }
        public bool isSaneTyeLength { set; get; }
    }
}