using System;
using System.Linq;
using LightQuery.Client;
using LightQuery.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LightQuery
{
    public class LightQueryAttribute :  ActionFilterAttribute
    {
        public LightQueryAttribute(bool forcePagination = false, int defaultPageSize = QueryParser.DEFAULT_PAGE_SIZE)
        {
            _forcePagination = forcePagination;
            _defaultPageSize = defaultPageSize;
        }

        private readonly bool _forcePagination;
        private readonly int _defaultPageSize;

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var queryContainer = ContextProcessor.GetQueryContainer(context, _defaultPageSize);
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
