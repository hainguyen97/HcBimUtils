namespace HcBimUtils.JsonData.MepTools.MepFilters
{
    public class MepFilterJson
    {
        public string ExcelPath { get; set; }
        public bool IsFilterForViewPlan { get; set; }
        public bool AddFiltersIfPossible { get; set; }

        public MepFilterJson()
        {
        }
    }
}