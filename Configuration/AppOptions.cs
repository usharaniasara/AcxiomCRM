namespace AcxiomCRM.Configuration
{
    public class AppOptions
    {
        public const string SectionName = "App";

        public string Name { get; set; } = "AcxiomCRM";
        public int PageSize { get; set; } = 10;
        public int ReportPageSize { get; set; } = 15;
    }
}
