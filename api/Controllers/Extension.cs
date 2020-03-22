using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using api.Controllers.Params;

namespace api.Controllers
{
    public static class Extension
    {

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, FilterParam param, bool ef = false) =>
            param == null? query : query.Filter(param.Column, param.Values, ef);
        private static IQueryable<T> Filter<T>(this IQueryable<T> query, string column, List<string> optionValues, bool ef = false)
        {
            var rowType = typeof(T);
            var columnProperty = rowType.GetProperty(column.ToTileCase()) ?? throw new ArgumentException($"can't find property {column.ToTileCase()} in type {rowType}");
            var rowParam = Expression.Parameter(rowType, "row");
            var columnExp = Expression.MakeMemberAccess(rowParam, columnProperty);
            var columnType = columnProperty.PropertyType;
            LambdaExpression filterExp;
            if (columnType == typeof(string))
            {
                var options = optionValues;
                Expression<Func<string, bool>> containsFunc = s =>  options.Contains(s);
                filterExp = Expression.Lambda(Expression.Invoke(Expression.Constant(containsFunc), columnExp), rowParam);

            } else if (columnType == typeof(bool))
            {
                var options = optionValues.Select(bool.Parse).ToList();
                Expression<Func<bool, bool>> containsFunc = s =>  options.Contains(s);
                filterExp = Expression.Lambda(Expression.Invoke(Expression.Constant(containsFunc), columnExp), rowParam);

            } else if (columnType == typeof(int))
            {
                var options = optionValues.Select(int.Parse).ToList();
                Expression<Func<int, bool>> containsFunc = s =>  options.Contains(s);
                filterExp = Expression.Lambda(Expression.Invoke(Expression.Constant(containsFunc), columnExp), rowParam);

            } else if (columnType == typeof(double))
            {
                var options = optionValues.Select(double.Parse).ToList();
                Expression<Func<double, bool>> containsFunc = s =>  options.Contains(s);
                filterExp = Expression.Lambda(Expression.Invoke(Expression.Constant(containsFunc), columnExp), rowParam);
            } else if (columnType == typeof(DateTime))
            {
                var options = optionValues.Select(DateTime.Parse).ToList();
                Expression<Func<DateTime, bool>> containsFunc = s =>  options.Contains(s);
                filterExp = Expression.Lambda(Expression.Invoke(Expression.Constant(containsFunc), columnExp), rowParam);
            }
            else
            {
                throw new ArgumentException($"can't parse string to type {columnType}");
            }
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "Where", new [] { rowType }, query.Expression, Expression.Quote(filterExp));
            return query.Provider.CreateQuery<T>(resultExp); 
        }

        private static IList ConvertTo(this List<string> lst, Type toType)
        {
            if (toType == typeof(string)) return lst;
            if (toType == typeof(bool)) return lst.Select(bool.Parse).ToList();
            if (toType == typeof(DateTime)) return lst.Select(DateTime.Parse).ToList();
            if (toType == typeof(int)) return lst.Select(int.Parse).ToList();
            throw new ArgumentException($"{toType} not supported");
        }

        public static string AsString(this string b)
        {
            return b;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, SortParam param)
            => param == null ? query : query.OrderBy(param.Column, param.Desc);

        private static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string column, bool desc)
        {
            var type = typeof(T);
            var property = type.GetProperty(column.ToTileCase()) ?? throw new ArgumentException($"Can't find Property {column.ToTileCase()} in {type} ");
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), desc? "OrderByDescending": "OrderBy", new [] { type, property.PropertyType }, query.Expression, Expression.Quote(orderByExp));
            return query.Provider.CreateQuery<T>(resultExp);
        }

        private static IQueryable<T> Range<T>(this IQueryable<T> query, int offset, int limit)
            => query.Skip(offset).Take(limit);

        public static IQueryable<T> Range<T>(this IQueryable<T> query, RangeParam range)
            => range == null? query : query.Range(range.OffSet, range.Limit);

        private static string ToTileCase(this string input) => input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input.First().ToString().ToUpper() + input.Substring(1)
        };

    }
    
}