// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectExtensions.cs" company="Bryan J. Ross">
//   (c) Bryan J. Ross. This code is provided as-is, with no warranty expressed or implied. Do with it as you will.
// </copyright>
// <summary>
//   Defines the ObjectExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gain
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static class ObjectExtensions
    {
        private static readonly ConcurrentDictionary<string, object> Creators = new ConcurrentDictionary<string, object>();

        public static TObj Change<TObj, TVal>(this TObj obj, Expression<Func<TObj, TVal>> expr, TVal newVal)
        {
            var bodyExpr = expr.Body as MemberExpression;
            if (bodyExpr == null)
            {
                throw Invalid();
            }

            var member = bodyExpr.Member;
            if (!(member is PropertyInfo || member is FieldInfo))
            {
                throw Invalid();
            }

            return GetObjectCreator<TObj, TVal>(member)(obj, newVal);
        }

        private static Func<TObj, TVal, TObj> GetObjectCreator<TObj, TVal>(MemberInfo member)
        {
            var key = "ObjectCreator:" + typeof(TObj).FullName + ":" + member.Name;
            var lambda = Creators.GetOrAdd(key, _ => CompileCreator<TObj, TVal>(member));
            return (Func<TObj, TVal, TObj>)lambda;
        }

        private static object CompileCreator<TObj, TVal>(MemberInfo member)
        {
            var ctor = FindConstructor<TObj>();
            var srcParam = Expression.Parameter(typeof(TObj), "src");
            var valParam = Expression.Parameter(typeof(TVal), "val");
            var args = ctor.GetParameters().Select(p => CreateArgExpression(typeof(TObj), p, srcParam, valParam, member));
            var newExpr = Expression.New(ctor, args);
            var expr = Expression.Lambda<Func<TObj, TVal, TObj>>(newExpr, srcParam, valParam);
            return expr.Compile();
        }

        private static Expression CreateArgExpression(Type type, ParameterInfo p, Expression srcParam, Expression valParam, MemberInfo memberToChange)
        {
            // Check if this is the one to change
            if (p.Name.Equals(memberToChange.Name, StringComparison.OrdinalIgnoreCase))
            {
                return valParam;
            }

            // Look for a matching property
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            var prop = type.GetProperty(p.Name, Flags);
            if (prop == null || !prop.CanRead)
            {
                var msg = string.Format(
                    "No matching property found for constructor argument {0} on type {1}", p.Name, type.FullName);
               throw new InvalidOperationException(msg);
            }

            return Expression.Convert(Expression.Property(srcParam, prop), p.ParameterType);
        }

        private static ConstructorInfo FindConstructor<T>()
        {
            var constructors = from constructor in typeof(T).GetConstructors()
                               orderby constructor.GetParameters().Length descending
                               select constructor;
            try
            {
                return constructors.First();
            }
            catch (InvalidOperationException ex)
            {
                var msg = string.Format("Type {0} does not provide any default constructors", typeof(T).FullName);
                throw new InvalidOperationException(msg, ex);
            }
        }

        private static InvalidOperationException Invalid()
        {
            return new InvalidOperationException("Must supply a property or field accessor expression to the Change extension method");
        }
    }
}
