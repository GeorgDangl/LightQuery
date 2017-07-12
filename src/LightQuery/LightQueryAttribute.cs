using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LightQuery
{
    public class LightQueryAttribute :  ActionFilterAttribute
    {
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
            var queryOptions = QueryParser.GetQueryOptions(context.HttpContext.Request.Query);
            if (string.IsNullOrWhiteSpace(queryOptions.SortPropertyName))
            {
                return;
            }
            var objectResult = context.Result as ObjectResult;
            var queryable = objectResult?.Value as IQueryable;
            if (queryable == null)
            {
                return;
            }
            var result = queryable.ApplySorting(queryOptions);
            objectResult.Value = result;
        }
    }
}
