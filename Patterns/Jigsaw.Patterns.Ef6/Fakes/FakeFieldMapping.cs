using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Jigsaw.Infrastructure.Ef6.Fakes
{
    public static class FakeFieldMapping
    {
        public static FakeFieldMapping<T> Create<T, TResult>(Expression<Func<T, TResult>> expression, Action<T, PropertyInfo> action)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            var property = ((MemberExpression)expression.Body).Member;
            var isValidMember = property.MemberType.HasFlag(MemberTypes.Property) ||
                                property.MemberType.HasFlag(MemberTypes.Field);
            if (isValidMember) {
                return new FakeFieldMapping<T>(property.Name, action);
            }

            throw new ArgumentException("should represent a Property or Field", "expression");
        }
    }
}