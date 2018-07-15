using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQueryable.Filter.Lib
{
    public static class FilterExtension
    {
        private static (ConstantExpression value, MemberExpression property) GetValueAndProperty(ParameterExpression parameter, string propertyName, object fieldValue)
        {
            var property = Expression.Property(parameter, propertyName);
            var value = Expression.Constant(Convert.ChangeType(fieldValue, property.Type));

            return (value, property);
        }

        private static Expression CreatePredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue,
            Func<Expression, Expression, BinaryExpression> binaryExpression)
        {
            var (value, property) = GetValueAndProperty(
                parameter,
                propertyName,
                fieldValue
            );

            return binaryExpression(value, property);
        }

        private static Expression ContainsPredicate(ParameterExpression parameter, string propertyName, object fieldValue)
        {
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var property = GetProperty();

            var value = Expression.Constant(Convert.ToString(fieldValue));

            return Expression.Call(
                    property,
                    containsMethod,
                    value
                );

            Expression GetProperty()
            {
                var propertyMaybeString = Expression.Property(parameter, propertyName);

                if (propertyMaybeString.Type != typeof(string))
                {
                    var toStringMethod = typeof(object).GetMethod("ToString");
                    return Expression.Call(propertyMaybeString, toStringMethod);
                }

                return propertyMaybeString;
            }
        }

        private static Expression EqualPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.Equal
        );

        private static Expression NotEqualPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.NotEqual
        );

        private static Expression LessThanPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.LessThan
        );

        private static Expression LessThanOrEqualPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.LessThanOrEqual
        );

        private static Expression GreaterThanPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.GreaterThan
        );

        private static Expression GreaterThanOrEqualPredicate(
            ParameterExpression parameter,
            string propertyName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            propertyName,
            fieldValue,
            Expression.GreaterThanOrEqual
        );

        private static Expression ApplyPredicate(ParameterExpression parameter, FilterPredicates? predicate, string propertyName, Type propertyType, object value)
        {
            switch (predicate)
            {
                case FilterPredicates.Contains:
                    {
                        return ContainsPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.Equal:
                    {
                        return EqualPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.NotEqual:
                    {
                        return NotEqualPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.LessThan:
                    {
                        return LessThanPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.LessThanOrEqual:
                    {
                        return LessThanOrEqualPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.GreaterThan:
                    {
                        return GreaterThanPredicate(parameter, propertyName, value);
                    }

                case FilterPredicates.GreaterThanOrEqual:
                    {
                        return GreaterThanOrEqualPredicate(parameter, propertyName, value);
                    }

                default:
                    return ContainsPredicate(parameter, propertyName, value);
            }
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, IEnumerable<FilterCondition> filterConditions) where T : class, new()
        {
            var type = typeof(T);

            var properties = type
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(FilterAttribute), false));

            var param = Expression.Parameter(typeof(T), "param");
            var filterExpression = (Expression)null;

            foreach (var filterCondition in filterConditions)
            {
                var property = properties
                    .Where(prop => FilterByFieldName(prop, filterCondition.FieldName))
                    .FirstOrDefault();

                if (property != null)
                {
                    if (filterExpression == null)
                    {
                        filterExpression = ApplyPredicate(
                            param,
                            filterCondition.Predicate,
                            property.Name,
                            property.PropertyType,
                            filterCondition.Value);
                    }
                    else
                    {
                        filterExpression = Expression.AndAlso(
                            filterExpression,
                            ApplyPredicate(
                                param,
                                filterCondition.Predicate,
                                property.Name,
                                property.PropertyType,
                                filterCondition.Value
                            )
                        );
                    }
                }
            }

            if (filterExpression != null)
            {
                Console.WriteLine(filterExpression);
                return query.Where(Expression.Lambda<Func<T, bool>>(filterExpression, param));
            }

            return query;

            bool FilterByFieldName(PropertyInfo propertyInfo, string fieldName)
                => propertyInfo.GetCustomAttribute<FilterAttribute>()
                    .FieldName == fieldName;
        }
    }
}