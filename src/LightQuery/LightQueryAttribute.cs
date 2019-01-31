using System.Linq;
using LightQuery.Client;
using LightQuery.Shared;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LightQuery
{
    public class LightQueryAttribute : ActionFilterAttribute
    {
        public LightQueryAttribute(bool forcePagination = false, int defaultPageSize = QueryParser.DEFAULT_PAGE_SIZE, string defaultSort = null)
        {
            _forcePagination = forcePagination;
            _defaultPageSize = defaultPageSize;
            _defaultSort = defaultSort;
            if (!_defaultSort.IsValidSortParameter())
            {
                throw new System.ArgumentException("Please specifiy either 'asc' or 'desc' as the sort direction for the defaultSort parameter and ensure it has not more than two segments", nameof(defaultSort));
            }
        }

        private readonly bool _forcePagination;
        private readonly int _defaultPageSize;
        private readonly string _defaultSort;

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var queryContainer = ContextProcessor.GetQueryContainer(context, _defaultPageSize, _defaultSort);
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

            var totalCount = queryable.Cast<object>().Count();
            if (totalCount <= ((queryOptions.Page - 1) * queryOptions.PageSize))
            {
                queryOptions.Page = (int)System.Math.Ceiling((decimal)totalCount / queryOptions.PageSize);
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
