namespace LightQuery.Shared
{
    public static class QueryStringExtensions
    {
        public static bool IsValidSortParameter(this string sortParam)
        {
            if (sortParam != null)
            {
                var split = sortParam.Split(' ');
                if (split.Length == 2)
                {
                    if (split[1].ToUpperInvariant() != "ASC"
                        && split[1].ToUpperInvariant() != "DESC")
                    {
                        return false;
                    }
                }
                if (split.Length > 2)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
