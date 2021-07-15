using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LightQuery.Shared
{
    public static class ContextProcessor
    {
        public static QueryContainer GetQueryContainer(ResultExecutingContext context,
            int defaultPageSize,
            string defaultSort,
            bool wrapNestedSortInNullChecks)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            var objectResult = context.Result as ObjectResult;
            var queryable = objectResult?.Value as IQueryable;
            if (queryable == null)
            {
                return new QueryContainer(null, null, null);
            }
            var queryOptions = QueryParser.GetQueryOptions(context.HttpContext.Request.Query, defaultPageSize, defaultSort);
            var sortedResult = queryOptions.Sort == null
                ? queryable
                : queryable.ApplySorting(queryOptions.Sort, queryOptions.ThenSort, wrapNestedSortInNullChecks);
            objectResult.Value = sortedResult;
            return new QueryContainer(objectResult, sortedResult, queryOptions);
        }
    }
}
