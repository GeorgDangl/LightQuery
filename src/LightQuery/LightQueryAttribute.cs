using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LightQuery
{
    public class LightQueryAttribute :  ActionFilterAttribute
    {
        public const int DEFAULT_PAGE_SIZE = 50;

        public LightQueryAttribute(bool forcePagination = false, int defaultPageSize = DEFAULT_PAGE_SIZE)
        {
            _forcePagination = forcePagination;
            _defaultPageSize = defaultPageSize;
        }

        private readonly bool _forcePagination;
        private readonly int _defaultPageSize;

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.HttpContext?.Request?.Query?.Any() != true)
            {
                return;
            }
            var objectResult = context.Result as ObjectResult;
            var queryable = objectResult?.Value as IQueryable;
            if (queryable == null)
            {
                return;
            }
            var queryOptions = QueryParser.GetQueryOptions(context.HttpContext.Request.Query, _defaultPageSize);
            var sortedResult = string.IsNullOrWhiteSpace(queryOptions.SortPropertyName)
                ? queryable
                : queryable.ApplySorting(queryOptions);
            if (_forcePagination || queryOptions.QueryRequestsPagination)
            {
                objectResult.Value = GetPaginationResult(sortedResult, queryOptions);
            }
            else
            {
                objectResult.Value = sortedResult;
            }
        }

        private PaginationResult<object> GetPaginationResult(IQueryable queryable, QueryOptions queryOptions)
        {

            return new PaginationResult<object>
            {
                Page = queryOptions.Page,
                PageSize = queryOptions.PageSize,
                TotalCount = queryable.Cast<object>().Count(),
                Data = queryable.Cast<object>().Skip((queryOptions.Page - 1) * queryOptions.PageSize).Take(queryOptions.PageSize).ToList()
            };
        }
    }
}
