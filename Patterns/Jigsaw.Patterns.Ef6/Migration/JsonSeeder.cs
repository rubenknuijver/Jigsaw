using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace Jigsaw.Infrastructure.Ef6
{
    public static class JsonSeeder
    {
        public static void FromStream<T>(this DbSet<T> dbSet, Stream stream, Expression<Func<T, object>> identifierExpression, params FieldMapping<T>[] additionalMapping) where T : class, IObjectState
        {
            using (var reader = new StreamReader(stream)) {
                var jsonString = reader.ReadToEnd();
                T[] modelCollection = JsonConvert.DeserializeObject<T[]>(jsonString);
                foreach (var entity in modelCollection) {
                    dbSet.Attach(entity);
                    dbSet.AddOrUpdate(identifierExpression, entity);
                    //StateHelper.ConvertState(entity.ObjectState);
                    //entity.ObjectState = ObjectState.Added;
                }
            }
        }

        public static void FromResource<T>(this DbSet<T> dbSet, string embeddedResourceName, Expression<Func<T, object>> identifierExpression, params FieldMapping<T>[] additionalMapping) where T : class,IObjectState
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(embeddedResourceName)) {
                FromStream(dbSet, stream, identifierExpression, additionalMapping);
            }
        }
    }

    public class FieldMapping<T>
    {
        private readonly string _fieldName;
        private readonly Action<T, object> _action;

        public string FieldName
        {
            get { return _fieldName; }
        }

        /// <summary>
        /// Create new custom mapping action
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="action"></param>
        public FieldMapping(string fieldName, Action<T, object> action)
        {
            _fieldName = fieldName;
            _action = action;
        }
        public static FieldMapping<T> Create<TResult>(Expression<Func<T, TResult>> fieldNameExpression, Action<T, object> action)
        {
            if (fieldNameExpression == null) throw new ArgumentNullException("fieldNameExpression");
            var property = ((MemberExpression)fieldNameExpression.Body).Member;
            var isValidMember = property.MemberType.HasFlag(MemberTypes.Property) ||
                                property.MemberType.HasFlag(MemberTypes.Field);
            if (isValidMember)
            {
                return new FieldMapping<T>(property.Name, action);
            }
            
            throw new ArgumentException("should represent a Property or Field", "fieldNameExpression");
        }


        public void Execute(T entity, object value)
        {
            _action(entity, value);
        }
    }
}
