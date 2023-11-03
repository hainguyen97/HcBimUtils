namespace HcBimUtils.JsonData.Beam.BeamAutoSection
{
    public class RecordModel
    {
        public bool IsParam { get; set; }
        public string Text { get; set; }
        public string PreviewText { get; set; }

        public RecordModel(string s, bool isParam)
        {
            IsParam = isParam;
            Text = s;
            PreviewText = s;
            if (isParam)
            {
                PreviewText = "{" + s + "}";
            }
        }
    }
}