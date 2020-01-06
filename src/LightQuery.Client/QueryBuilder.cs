using System;
using System.Collections.Generic;
using System.Linq;

namespace LightQuery.Client
{
    public static class QueryBuilder
    {
        public static string Build(int page = 1,
            int pageSize = 20,
            string sortParam = null,
            bool sortDescending = false,
            string thenSortParam = null,
            bool thenSortDescending = false,
            Dictionary<string, string> additionalParameters = null)
        {
            if (page <= 0)
            {
                throw new ArgumentException($"{nameof(page)} must be bigger than zero", nameof(page));
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException($"{nameof(pageSize)} must be bigger than zero", nameof(pageSize));
            }
            var query = $"?page={page}" +
                        $"&pageSize={pageSize}"
                        + BuildSortParameter(sortParam, sortDescending)
                        + BuildThenSortParameter(thenSortParam, thenSortDescending)
                        + BuildAdditionalParameters(additionalParameters);
            return query;
        }

        private static string BuildSortParameter(string sortParam, bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortParam))
            {
                return string.Empty;
            }
            var sortQuery = $"&sort={Uri.EscapeUriString(sortParam)}%20" + (sortDescending ? "desc" : "asc");
            return sortQuery;
        }

        private static string BuildThenSortParameter(string thenSortParam, bool thenSortDescending)
        {
            if (string.IsNullOrWhiteSpace(thenSortParam))
            {
                return string.Empty;
            }
            var sortQuery = $"&thenSort={Uri.EscapeUriString(thenSortParam)}%20" + (thenSortDescending ? "desc" : "asc");
            return sortQuery;
        }

        private static string BuildAdditionalParameters(Dictionary<string, string> additionalParameters)
        {
            if (additionalParameters == null || !additionalParameters.Any())
            {
                return string.Empty;
            }
            var query = string.Empty;
            foreach (var parameter in additionalParameters)
            {
                query += $"&{Uri.EscapeUriString(parameter.Key)}" + (string.IsNullOrWhiteSpace(parameter.Value) ? string.Empty : $"={Uri.EscapeUriString(parameter.Value)}");
            }
            return query;
        }
    }
}
