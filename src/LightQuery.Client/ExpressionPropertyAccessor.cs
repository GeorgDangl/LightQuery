using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LightQuery.Client
{
    public static class ExpressionPropertyAccessor
    {
        public static string GetPropertyNameFromExpression<T, TKey>(Expression<Func<T, TKey>> property)
        {
            var parameterName = string.Empty;
            var expressionBody = property.Body;
            if (expressionBody is MemberExpression memberExpression)
            {
                parameterName = memberExpression.Member.Name;
            }
            if (expressionBody is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                parameterName = unaryMemberExpression.Member.Name;
            }
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                var typeName = typeof(T).GetTypeInfo().Name;
                throw new ArgumentException($"The expression must be a property accessor on {typeName}, e.g. \"t => t.Property\"", nameof(property));
            }
            return parameterName;
        }

        public static bool PropertyExistsOnType<T>(string parameterName)
        {
            var propertyExists = typeof(T).GetTypeInfo().DeclaredProperties.Any(p => p.Name == parameterName);
            return propertyExists;
        }
    }
}
