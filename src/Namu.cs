using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace Namu
{
    class ReflectionUtility
    {
        public static MethodBase Caller
        {
            get
            {
                StackFrame frame = new StackFrame(2);
                return frame.GetMethod();
            }
        }
        public static MethodBase Caller2
        {
            get
            {
                StackFrame frame = new StackFrame(3);
                return frame.GetMethod();
            }
        }
    }

    public sealed class Mock
    {
        public static Selector<T> Select<T>()
        {
            return new Selector<T>();
        }
        public static Selector<T> Select<T>(T obj)
        {
            return new Selector<T>();
        }

        public static BoundMethod Method(Expression<Action> methodLambda)
        {
            MethodCallExpression call = methodLambda.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property, not a method.",
                    methodLambda.ToString()));

            return new BoundMethod(call.Method);
        }

        public static bool IsRegistered
        {
            get
            {
                var caller = ReflectionUtility.Caller;
                bool registered = false;

                lock (mocks)
                {
                    registered = mocks.ContainsKey(GetPath(caller));
                }

                return registered;
            }
        }
        public static bool IsRegisteredTx
        {
            get
            {
                var caller = ReflectionUtility.Caller;

                // see https://msdn.microsoft.com/en-us/library/ms173179(v=vs.80).aspx
                try
                {
                    Monitor.Enter(mocks);

                    return mocks.ContainsKey(GetPath(caller));
                }
                catch
                {
                    Monitor.Exit(mocks);
                }
                return false;
            }
        }
        public static object Value
        {
            get
            {
                if (Monitor.IsEntered(mocks))
                    return ValueTx;

                var caller = ReflectionUtility.Caller;

                lock (mocks)
                {
                    return mocks[GetPath(caller)]();
                }
            }
        }
        private static object ValueTx
        {
            get
            {
                try
                {
                    var caller = ReflectionUtility.Caller2;

                    return mocks[GetPath(caller)]();
                }
                finally
                {
                    Monitor.Exit(mocks);
                }
            }
        }

        public static bool TryGetValue(out object value)
        {
            var caller = ReflectionUtility.Caller;
            var path = GetPath(caller);
            value = null;

            lock (mocks)
            {
                if (mocks.ContainsKey(path) == false)
                    return false;

                value = mocks[path]();
            }
            return true;
        }

        private static Dictionary<string, Func<object>> mocks { get; set; }

        static Mock()
        {
            mocks = new Dictionary<string, Func<object>>();
        }

        internal static string GetPath(MethodBase method)
        {
            return $"{method.DeclaringType.FullName}::{method.Name}";
        }
        internal static string GetPath(PropertyInfo property)
        {
            if (property.CanWrite)
                throw new ArgumentException("Property is setter");
            return $"{property.DeclaringType.FullName}::get_{property.Name}";
        }

        internal static void Bind(string path, object value)
        {
            lock (mocks)
            {
                mocks[path] = () => value;
            }
        }
        internal static void Bind(string path, Func<object> generator)
        {
            lock (mocks)
            {
                mocks[path] = generator;
            }
        }

        internal static void Unbind(string path)
        {
            lock (mocks)
            {
                mocks.Remove(path);
            }
        }
        public static void Claer()
        {
            lock (mocks)
            {
                mocks.Clear();
            }
        }
    }

    public sealed class Selector<T>
    {
        public Selector()
        {
        }
        
        public BoundProperty<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertyLambda)
        {
            Type type = typeof(T);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo property = member.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != property.ReflectedType &&
                !type.IsSubclassOf(property.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return new BoundProperty<T, TProperty>(property);
        }
        public BoundMethod Method(Expression<Action<T>> methodLambda)
        {
            MethodCallExpression call = methodLambda.Body as MethodCallExpression;
            if (call == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property, not a method.",
                    methodLambda.ToString()));

            return new BoundMethod(call.Method);
        }
    }

    public sealed class BoundProperty<T, TProperty>
    {
        private PropertyInfo property { get; set; }

        public BoundProperty(PropertyInfo property)
        {
            this.property = property;
        }

        public void Should(object value)
        {
            Mock.Bind(Mock.GetPath(property), value);
        }
        public void Should(Func<object> generator)
        {
            Mock.Bind(Mock.GetPath(property), generator);
        }
        public void Unbind()
        {
            Mock.Unbind(Mock.GetPath(property));
        }
    }
    public sealed class BoundMethod
    {
        private MethodInfo method { get; set; }

        public BoundMethod(MethodInfo method)
        {
            this.method = method;
        }

        public void Should(object value)
        {
            Mock.Bind(Mock.GetPath(method), value);
        }
        public void Should(Func<object> generator)
        {
            Mock.Bind(Mock.GetPath(method), generator);
        }
        public void Unbind()
        {
            Mock.Unbind(Mock.GetPath(method));
        }
    }
}