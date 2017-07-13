namespace LightQuery
{
    public class QueryOptions
    {
        public string SortPropertyName { get; set; }
        public bool IsDescending { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; }
        public bool QueryRequestsPagination { get; set; }
    }
}
