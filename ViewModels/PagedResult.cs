namespace AcxiomCRM.ViewModels
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? ExtraFilter { get; set; }
        public string? AssignedToUserId { get; set; }
    }
}
