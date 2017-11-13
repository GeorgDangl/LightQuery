namespace LightQuery.Client
{
    public class DefaultPaginationOptions
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SortProperty { get; set; }
        public bool SortDescending { get; set; }
    }
}
