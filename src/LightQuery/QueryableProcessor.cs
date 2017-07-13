using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LightQuery
{
    public static class QueryableProcessor
    {
        public static IQueryable ApplySorting(this IQueryable queryable, QueryOptions queryOptions)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }
            if (queryOptions == null)
            {
                throw new ArgumentNullException(nameof(queryOptions));
            }
            if (string.IsNullOrWhiteSpace(queryOptions.SortPropertyName))
            {
                return queryable;
            }
            var orderingProperty = queryable.ElementType.GetTypeInfo().GetProperty(queryOptions.SortPropertyName);
            if (orderingProperty == null)
            {
                return queryable;
            }
            var parameter = Expression.Parameter(queryable.ElementType, "v");
            var propertyAccess = Expression.MakeMemberAccess(parameter, orderingProperty);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            var orderMethodName = queryOptions.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var wrappedExpression = Expression.Call(typeof(Queryable), orderMethodName, new [] { queryable.ElementType, orderingProperty.PropertyType }, queryable.Expression, Expression.Quote(orderByExp));
            var result = queryable.Provider.CreateQuery(wrappedExpression);
            return result;
        }
    }
}
