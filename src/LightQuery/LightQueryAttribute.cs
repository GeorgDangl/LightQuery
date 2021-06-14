using LightQuery.Client;
using LightQuery.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace LightQuery
{
    public class LightQueryAttribute : ActionFilterAttribute
    {
        public LightQueryAttribute(bool forcePagination = false,
            int defaultPageSize = QueryParser.DEFAULT_PAGE_SIZE,
            string defaultSort = null,
            bool wrapNestedSortInNullChecks = true)
        {
            _forcePagination = forcePagination;
            _defaultPageSize = defaultPageSize;
            _defaultSort = defaultSort;
            _wrapNestedSortInNullChecks = wrapNestedSortInNullChecks;
            if (!_defaultSort.IsValidSortParameter())
            {
                throw new System.ArgumentException("Please specifiy either 'asc' or 'desc' as the sort direction for the defaultSort parameter and ensure it has not more than two segments", nameof(defaultSort));
            }
        }

        private readonly bool _forcePagination;
        private readonly int _defaultPageSize;
        private readonly string _defaultSort;
        private readonly bool _wrapNestedSortInNullChecks;

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context?.Result is StatusCodeResult statusCodeResult
               && (statusCodeResult.StatusCode < 200
               || statusCodeResult.StatusCode >= 300))
            {
                return;
            }

            var queryContainer = ContextProcessor.GetQueryContainer(context,
                _defaultPageSize,
                _defaultSort,
                _wrapNestedSortInNullChecks);
            if (queryContainer.ObjectResult == null)
            {
                return;
            }
            if (_forcePagination || queryContainer.QueryOptions.QueryRequestsPagination)
            {
                queryContainer.ObjectResult.Value = GetPaginationResult(queryContainer);
            }
        }

        private PaginationResult<object> GetPaginationResult(QueryContainer queryContainer)
        {
            var queryOptions = queryContainer.QueryOptions;
            var queryable = queryContainer.Queryable;

            dynamic dynamicQueryable = queryable;
            var totalCount = Queryable.Count(dynamicQueryable);
            if (totalCount <= ((queryOptions.Page - 1) * queryOptions.PageSize))
            {
                queryOptions.Page = (int)System.Math.Ceiling((decimal)totalCount / queryOptions.PageSize);
                queryOptions.Page = queryOptions.Page == 0 ? 1 : queryOptions.Page;
            }

            return new PaginationResult<object>
            {
                Page = queryOptions.Page,
                PageSize = queryOptions.PageSize,
                TotalCount = totalCount,
                Data = queryable.Cast<object>().Skip((queryOptions.Page - 1) * queryOptions.PageSize).Take(queryOptions.PageSize).ToList()
            };
        }
    }
}
