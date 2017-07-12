using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace LightQuery
{
    public class QueryParser
    {
        public static QueryOptions GetQueryOptions(IQueryCollection query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }
            var queryOptions = new QueryOptions();
            var sortParam = query["sort"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(sortParam))
            {
                return queryOptions;
            }
            var paramSections = sortParam.Split(' ').ToList();
            if (paramSections.Count > 2)
            {
                // Invalid format -> Return no decision instead of a wrong one
                return queryOptions;
            }
            queryOptions.SortPropertyName = CamelizeString(paramSections[0]);
            if (paramSections.Count == 2)
            {
                queryOptions.IsDescending = IsDescendingSortParameter(paramSections[1]);
            }
            return queryOptions;
        }

        private static string CamelizeString(string camelCase)
        {
            return camelCase.Substring(0, 1).ToUpperInvariant() + camelCase.Substring(1);
        }

        private static bool IsDescendingSortParameter(string sortIndicator)
        {
            var indicatesDescending = sortIndicator
                .ToUpperInvariant()
                .Contains("desc".ToUpperInvariant());
            return indicatesDescending;
        }
    }
}
