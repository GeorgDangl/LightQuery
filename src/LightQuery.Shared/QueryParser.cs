using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace LightQuery.Shared
{
    public static class QueryParser
    {
        public const int DEFAULT_PAGE_SIZE = 50;
        
        public static QueryOptions GetQueryOptions(IQueryCollection query, int defaultPageSize = DEFAULT_PAGE_SIZE, string defaultSort = null)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            var queryOptions = new QueryOptions();
            ParseSortingOptions(queryOptions, query, defaultSort);
            ParsePagingOptions(queryOptions, query, defaultPageSize);
            return queryOptions;
        }

        private static void ParseSortingOptions(QueryOptions queryOptions, IQueryCollection query, string defaultSort)
        {
            var sortParam = string.IsNullOrWhiteSpace(query["sort"].FirstOrDefault()) ? defaultSort : query["sort"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(sortParam))
            {
                return;
            }
            var paramSections = sortParam.Split(' ').ToList();
            if (paramSections.Count > 2)
            {
                // Invalid format -> Return no decision instead of a wrong one
                return;
            }
            queryOptions.SortPropertyName = paramSections[0];
            if (paramSections.Count == 2)
            {
                queryOptions.IsDescending = IsDescendingSortParameter(paramSections[1]);
            }
        }

        private static bool IsDescendingSortParameter(string sortIndicator)
        {
            var indicatesDescending = sortIndicator
                .ToUpperInvariant()
                .Contains("desc".ToUpperInvariant());
            return indicatesDescending;
        }

        private static void ParsePagingOptions(QueryOptions options, IQueryCollection query, int defaultPageSize)
        {
            var pageParam = query["page"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pageParam) && Int32.TryParse(pageParam, out var parsedPage))
            {
                options.QueryRequestsPagination = true;
                options.Page = Math.Max(1, parsedPage);
            }
            var pageSizeParam = query["pageSize"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(pageSizeParam) && Int32.TryParse(pageSizeParam, out var parsedPageSize))
            {
                options.QueryRequestsPagination = true;
                options.PageSize = parsedPageSize > 0
                    ? parsedPageSize
                    : defaultPageSize;
            }
            else
            {
                options.PageSize = defaultPageSize;
            }
        }
    }
}
