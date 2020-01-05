namespace LightQuery.Shared
{
    public class QueryOptions
    {
        public SortOption Sort { get; set; }
        public SortOption ThenSort { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
        public bool QueryRequestsPagination { get; set; }
    }
}
