using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQueryable.Filter.Lib
{
    public static class FilterExtension
    {
        private static bool FilterByFieldName(PropertyInfo propertyInfo, string fieldName)
                => propertyInfo.GetCustomAttribute<FilterAttribute>()
                    .FieldName == fieldName;
        private static MemberExpression GetProperty(Expression left, string fieldName)
        {
            var propertyInfos = left
                .Type
                .GetProperties()
                .Where(prop => prop.IsDefined(typeof(FilterAttribute), false));

            var dotIndex = fieldName.IndexOf(".");
            var propertyInfo = GetPropertyInfo();

            if (dotIndex > 0)
            {
                return GetProperty(
                    Expression.Property(left, propertyInfo.Name),
                    fieldName.Substring(dotIndex + 1));
            }

            return Expression.Property(left, propertyInfo.Name);

            PropertyInfo GetPropertyInfo()
            {
                if (dotIndex > 0)
                {
                    return propertyInfos
                        .Where(prop => FilterByFieldName(prop, fieldName.Substring(0, dotIndex)))
                        .FirstOrDefault();
                }

                return propertyInfos
                    .Where(prop => FilterByFieldName(prop, fieldName))
                    .FirstOrDefault();
            }
        }

        private static (ConstantExpression value, MemberExpression property) GetValueAndProperty(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue)
        {
            var property = GetProperty(parameter, fieldName);
            var value = Expression.Constant(Convert.ChangeType(fieldValue, property.Type));

            return (value, property);
        }

        private static Expression GetPropertyEnsureIsString(ParameterExpression parameter, string fieldName)
        {
            var propertyMaybeString = GetProperty(parameter, fieldName);

            if (propertyMaybeString.Type != typeof(string))
            {
                var toStringMethod = typeof(object).GetMethod("ToString");
                return Expression.Call(propertyMaybeString, toStringMethod);
            }

            return propertyMaybeString;
        }

        private static Expression CreatePredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue,
            Func<Expression, Expression, BinaryExpression> binaryExpression)
        {
            var (value, property) = GetValueAndProperty(
                parameter,
                fieldName,
                fieldValue
            );

            return binaryExpression(value, property);
        }

        private static Expression ContainsPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue)
        {
            var containsMethod = typeof(string)
                .GetMethod("Contains", new[] { typeof(string) });
            var property = GetPropertyEnsureIsString(parameter, fieldName);

            var value = Expression.Constant(Convert.ToString(fieldValue));

            return Expression.Call(
                    property,
                    containsMethod,
                    value
                );
        }

        private static Expression StartsWithPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue)
        {
            var containsMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            var property = GetPropertyEnsureIsString(parameter, fieldName);

            var value = Expression.Constant(Convert.ToString(fieldValue));

            return Expression.Call(
                    property,
                    containsMethod,
                    value
                );
        }

        private static Expression EndsWithPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue)
        {
            var containsMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });
            var property = GetPropertyEnsureIsString(parameter, fieldName);

            var value = Expression.Constant(Convert.ToString(fieldValue));

            return Expression.Call(
                    property,
                    containsMethod,
                    value
                );
        }

        private static Expression OneOfPredicate(
            ParameterExpression parameter,
            string fieldName,
            List<object> fieldValues)
        {
            var containsMethod = typeof(List<object>)
                .GetMethod("Contains", new[] { typeof(object) });
            var property = GetProperty(parameter, fieldName);

            var values = Expression.Constant(
                fieldValues
                    .Select(fieldValue =>
                        Convert.ChangeType(fieldValue, property.Type)
                    )
                    .ToList());

            return Expression.Call(
                    values,
                    containsMethod,
                    Expression.Convert(
                        property,
                        typeof(object)
                    )
                );
        }

        private static Expression EqualPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.Equal
        );

        private static Expression NotEqualPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.NotEqual
        );

        private static Expression LessThanPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.LessThan
        );

        private static Expression LessThanOrEqualPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.LessThanOrEqual
        );

        private static Expression GreaterThanPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.GreaterThan
        );

        private static Expression GreaterThanOrEqualPredicate(
            ParameterExpression parameter,
            string fieldName,
            object fieldValue
        ) => CreatePredicate(
            parameter,
            fieldName,
            fieldValue,
            Expression.GreaterThanOrEqual
        );

        private static Expression ApplyPredicate(
            ParameterExpression parameter,
            FilterCondition filterCondition)
        {
            switch (filterCondition.Predicate)
            {
                case FilterPredicates.Contains:
                    {
                        return ContainsPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.StartsWith:
                    {
                        return StartsWithPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.EndsWith:
                    {
                        return EndsWithPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.Equal:
                    {
                        return EqualPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.NotEqual:
                    {
                        return NotEqualPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.LessThan:
                    {
                        return LessThanPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.LessThanOrEqual:
                    {
                        return LessThanOrEqualPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.GreaterThan:
                    {
                        return GreaterThanPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.GreaterThanOrEqual:
                    {
                        return GreaterThanOrEqualPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
                    }

                case FilterPredicates.OneOf:
                    {
                        return OneOfPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Values);
                    }

                default:
                    return ContainsPredicate(
                            parameter,
                            filterCondition.FieldName,
                            filterCondition.Value);
            }
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> query, IEnumerable<FilterCondition> filterConditions) where T : class, new()
        {
            var param = Expression.Parameter(typeof(T), "param");
            var filterExpression = (Expression)null;

            foreach (var filterCondition in filterConditions)
            {
                if (filterExpression == null)
                {
                    filterExpression = ApplyPredicate(
                        param,
                        filterCondition);
                }
                else
                {
                    filterExpression = Expression.AndAlso(
                        filterExpression,
                        ApplyPredicate(
                            param,
                            filterCondition
                        )
                    );
                }
            }

            if (filterExpression != null)
            {
                return query.Where(Expression.Lambda<Func<T, bool>>(filterExpression, param));
            }

            return query;
        }
    }
}