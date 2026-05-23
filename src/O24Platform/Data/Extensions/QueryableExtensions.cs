using System.Linq.Expressions;

namespace O24OpenAPI.Data.Extensions;

public static class LinqToDbExtensions
{
    public static IQueryable<T> WhereContainsText<T>(
        this IQueryable<T> source,
        Expression<Func<T, object>> fields,
        string searchText
    )
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return source;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression body = null;

        foreach (var propertyName in GetPropertyNames(fields))
        {
            var member = Expression.Property(parameter, propertyName);

            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));

            var contains = Expression.Call(
                member,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(searchText)
            );

            var condition = Expression.AndAlso(notNull, contains);

            body = body == null ? condition : Expression.OrElse(body, condition);
        }

        if (body == null)
            return source;

        var predicate = Expression.Lambda<Func<T, bool>>(body, parameter);
        return source.Where(predicate);
    }

    private static IEnumerable<string> GetPropertyNames<T>(Expression<Func<T, object>> expression)
    {
        if (expression.Body is NewExpression newExpr)
            return newExpr.Members!.Select(m => m.Name);

        throw new ArgumentException("Expression must be an anonymous object");
    }
}
