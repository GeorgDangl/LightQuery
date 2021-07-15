using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LightQuery.Shared
{
    public static class QueryableProcessor
    {
        private static (Type declaringType, PropertyInfo property) GetPropertyInfoRecursively(this IQueryable queryable, string propName)
        {
            var nameParts = propName.Split('.');
            if (nameParts.Length == 1)
            {
                var property = queryable.ElementType.GetTypeInfo().GetProperty(CamelizeString(propName)) ?? queryable.ElementType.GetTypeInfo().GetProperty(propName);
                return (property?.DeclaringType, property);
            }

            //Getting Root Property - Ex : propName : "User.Name" -> User
            var propertyInfo = queryable.ElementType.GetTypeInfo().GetProperty(CamelizeString(nameParts[0])) ?? queryable.ElementType.GetTypeInfo().GetProperty(nameParts[0]);
            if (propertyInfo == null)
            {
                return (null, null);
            }

            var originalDeclaringType = propertyInfo.DeclaringType;
            for (int i = 1; i < nameParts.Length; i++)
            {
                propertyInfo = propertyInfo.PropertyType.GetProperty(CamelizeString(nameParts[i])) ?? propertyInfo.PropertyType.GetProperty(nameParts[i]);
                if (propertyInfo == null)
                {
                    return (null, null);
                }
            }
            return (originalDeclaringType, propertyInfo);
        }

        private static LambdaExpression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "v");
            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, CamelizeString(member)) ?? Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda(body, param);
        }

        public static IQueryable ApplySorting(this IQueryable queryable,
            SortOption sortOption,
            SortOption thenSortOption,
            bool wrapNestedSortInNullChecks = true)
        {
            if (queryable == null)
            {
                throw new ArgumentNullException(nameof(queryable));
            }

            if (sortOption == null)
            {
                throw new ArgumentNullException(nameof(sortOption));
            }

            if (string.IsNullOrWhiteSpace(sortOption.PropertyName))
            {
                return queryable;
            }

            var orderMethodName = sortOption.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);
            var result = ApplySorting(queryable, orderMethodName, sortOption.PropertyName, wrapNestedSortInNullChecks);

            if (thenSortOption != null
                && !string.IsNullOrWhiteSpace(thenSortOption.PropertyName)
                && !thenSortOption.PropertyName.Contains('.')) // A point '.' in the thenSort property is not allowed, due to relational sorting
                // only working on the regular sort property
            {
                var thenOrderMethodName = thenSortOption.IsDescending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy);
                result = ApplySorting(result, thenOrderMethodName, thenSortOption.PropertyName, wrapNestedSortInNullChecks);
            }

            return result;
        }

        private static IQueryable ApplySorting(IQueryable queryable, string sortMethodName, string propertyName, bool wrapNestedSortInNullChecks)
        {
            var orderingProperty = GetPropertyInfoRecursively(queryable, propertyName);
            if (orderingProperty.declaringType == null
                || orderingProperty.property == null)
            {
                return queryable;
            }

            var orderByExp = CreateExpression(orderingProperty.declaringType, propertyName);
            if (orderByExp == null)
            {
                return queryable;
            }

            if (wrapNestedSortInNullChecks)
            {
                // If this is a nested expression, it should additionally add null checks to exclude null children
                queryable = queryable.WrapInNullChecksIfAccessingNestedProperties(queryable.ElementType, propertyName);
            }
            var wrappedExpression = Expression.Call(typeof(Queryable),
                sortMethodName,
                new[] { orderingProperty.declaringType, orderingProperty.property.PropertyType },
                queryable.Expression,
                Expression.Quote(orderByExp));
            var result = queryable.Provider.CreateQuery(wrappedExpression);
            return result;
        }

        private static IQueryable WrapInNullChecksIfAccessingNestedProperties(this IQueryable queryable, Type type, string propertyName)
        {
            var members = propertyName.Split('.');
            if (members.Length == 1)
            {
                return queryable;
            }

            // The following is essentially just appending a .Where() clause
            // to the queryable for each depth level of the query, e.g. for "Product.Data.Title"
            // it generates:
            // queryable
            //  .Where(x => x.Product != null)
            //  .Where(x => x.Product.Data != null)
            for (var i = 0; i < members.Length - 1; i++)
            {
                var member = members[i];
                var param = Expression.Parameter(type, "v");
                Expression body = param;
                for (var j = 0; j <= i; j++)
                {
                    body = Expression.PropertyOrField(body, CamelizeString(members[j])) ?? Expression.PropertyOrField(body, members[j]);
                }

                var memberPath = members
                    .TakeWhile((mem, index) => index <= i)
                    .Aggregate((c, n) => c + "." + n);
                var notNullExpression = Expression.NotEqual(body, Expression.Constant(null));
                var notNullLambda = Expression.Lambda(notNullExpression, param);
                var whereMethodName = nameof(Queryable.Where);
                var nullCheckExpression = Expression.Call(typeof(Queryable), whereMethodName, new[] { type }, queryable.Expression, Expression.Quote(notNullLambda));
                queryable = queryable.Provider.CreateQuery(nullCheckExpression);
            }

            return queryable;
        }

        private static string CamelizeString(string camelCase)
        {
            return camelCase.Substring(0, 1).ToUpperInvariant() + camelCase.Substring(1);
        }
    }
}
