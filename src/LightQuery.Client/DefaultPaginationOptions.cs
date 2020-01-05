namespace LightQuery.Client
{
    public class DefaultPaginationOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortProperty { get; set; }
        public bool SortDescending { get; set; }
        public string ThenSortProperty { get; set; }
        public bool ThenSortDescending { get; set; }
    }
}
