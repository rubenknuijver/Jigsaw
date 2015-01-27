using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Jigsaw.Infrastructure.Ef6.Fakes
{
    public class FakeFieldMapping<T>
    {
        private readonly string _fieldName;
        private readonly Action<T, PropertyInfo> _action;

        public string FieldName
        {
            get { return _fieldName; }
        }

        /// <summary>
        /// Create new custom mapping action
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="action"></param>
        public FakeFieldMapping(string fieldName, Action<T, PropertyInfo> action)
        {
            _fieldName = fieldName;
            _action = action;
        }

        public void Execute(T entity, PropertyInfo value)
        {
            _action(entity, value);
        }

        public static FakeFieldMapping<T> Create<TResult>(Expression<Func<T, TResult>> expression, Action<T, PropertyInfo> action)
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