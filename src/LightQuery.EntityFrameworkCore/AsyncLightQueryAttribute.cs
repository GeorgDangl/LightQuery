using LightQuery.Client;
using LightQuery.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace LightQuery.EntityFrameworkCore
{
    public class AsyncLightQueryAttribute : ActionFilterAttribute
    {
        public AsyncLightQueryAttribute(bool forcePagination = false, int defaultPageSize = QueryParser.DEFAULT_PAGE_SIZE, string defaultSort = null)
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

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context?.Result is StatusCodeResult statusCodeResult
                && (statusCodeResult.StatusCode < 200
                || statusCodeResult.StatusCode >= 300))
            {
                await next.Invoke();
                return;
            }

            var queryContainer = ContextProcessor.GetQueryContainer(context, _defaultPageSize, _defaultSort);
            if (queryContainer.ObjectResult == null)
            {
                await next.Invoke();
                return;
            }
            if (_forcePagination || queryContainer.QueryOptions.QueryRequestsPagination)
            {
                queryContainer.ObjectResult.Value = await GetPaginationResult(queryContainer);
            }
            else
            {
                queryContainer.ObjectResult.Value = await queryContainer.Queryable.Cast<object>().ToListAsync();
            }
            await next.Invoke();
        }

        private async Task<PaginationResult<object>> GetPaginationResult(QueryContainer queryContainer)
        {
            var queryOptions = queryContainer.QueryOptions;
            var queryable = queryContainer.Queryable;

            dynamic dynamicQueryable = queryable;
            var totalCount = await EntityFrameworkQueryableExtensions.CountAsync(dynamicQueryable);
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
                Data = await queryable.Cast<object>().Skip((queryOptions.Page - 1) * queryOptions.PageSize).Take(queryOptions.PageSize).ToListAsync()
            };
        }
    }
}
