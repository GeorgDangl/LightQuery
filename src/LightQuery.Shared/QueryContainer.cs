using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace LightQuery.Shared
{
    public class QueryContainer
    {
        public QueryContainer(ObjectResult objectResult,
            IQueryable queryable,
            QueryOptions queryOptions)
        {
            ObjectResult = objectResult;
            Queryable = queryable;
            QueryOptions = queryOptions;
        }
        public ObjectResult ObjectResult { get; }
        public IQueryable Queryable { get; }
        public QueryOptions QueryOptions { get; }
    }
}
