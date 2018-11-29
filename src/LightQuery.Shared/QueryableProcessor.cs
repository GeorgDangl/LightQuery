using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LightQuery.Shared
{
    public static class QueryableProcessor
    {
        public static PropertyInfo GetPropertyInfoRecursivly(this IQueryable queryable, String propName)
        {
            string[] nameParts = propName.Split('.');
            if (nameParts.Length == 1)
            {
                return queryable.ElementType.GetTypeInfo().GetProperty(CamelizeString(propName)) ?? queryable.ElementType.GetTypeInfo().GetProperty(propName);
            }

            //Getting Root Property - Ex : propName : "User.Name" -> User
            var propertyInfo = queryable.ElementType.GetTypeInfo().GetProperty(CamelizeString(nameParts[0])) ?? queryable.ElementType.GetTypeInfo().GetProperty(nameParts[0]);
            if (propertyInfo == null)
            {
                return null;
            }

            for (int i = 1; i < nameParts.Length; i++)
            {
                propertyInfo = propertyInfo.PropertyType.GetProperty(CamelizeString(nameParts[i])) ?? propertyInfo.PropertyType.GetProperty(nameParts[i]);
                if (propertyInfo == null)
                {
                    return null;
                }
            }
            return propertyInfo;
        }

        public static LambdaExpression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "v");
            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, CamelizeString(member)) ?? Expression.PropertyOrField(body, member);
            }
            return Expression.Lambda(body, param);
        }

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

            var orderingProperty = GetPropertyInfoRecursivly(queryable, queryOptions.SortPropertyName);
            if (orderingProperty == null)
            {
                return queryable;
            }

            var orderByExp = CreateExpression(queryable.ElementType, queryOptions.SortPropertyName);
            if (orderByExp == null)
            {
                return queryable;
            }
            var orderMethodName = queryOptions.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var wrappedExpression = Expression.Call(typeof(Queryable), orderMethodName, new [] { queryable.ElementType, orderingProperty.PropertyType }, queryable.Expression, Expression.Quote(orderByExp));
            var result = queryable.Provider.CreateQuery(wrappedExpression);
            return result;
        }

        private static string CamelizeString(string camelCase)
        {
            return camelCase.Substring(0, 1).ToUpperInvariant() + camelCase.Substring(1);
        }
    }
}
