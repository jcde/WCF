using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace WcfDomain.Threads
{
    public class ComplexMonitor : IDisposable
    {
        private static readonly Dictionary<Type, List<MemberInfo>> _propLists
            = new Dictionary<Type, List<MemberInfo>>();

        private static readonly Dictionary<Type, List<MemberInfo>> _props
            = new Dictionary<Type, List<MemberInfo>>();

        internal object _LockedObj;

        public ComplexMonitor(object obj)
        {
            Enter(obj);
            _LockedObj = obj;
        }

        #region DamaMember getting/setting

        internal static List<MemberInfo> GetProps(Type t)
        {
            return GetProps(t, true);
        }

        private static List<MemberInfo> GetProps(Type t, bool listOnly)
        {
            List<MemberInfo> l;
            Dictionary<Type, List<MemberInfo>> props = listOnly ? _propLists : _props;
            if (!props.ContainsKey(t))
            {
                l = new List<MemberInfo>();
                foreach (MemberInfo pr in t.GetMembers(
                    BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
                    if (pr.GetCustomAttributes(typeof (DataMemberAttribute), true).Length > 0)
                    {
                        if (!listOnly || IsList(pr))
                            l.Add(pr);
                    }
                props.Add(t, l);
            }
            else
                l = props[t];
            return l;
        }

        private static List<object> GetPropValues(Type t, bool listOnly, object obj)
        {
            var l = new List<object>();
            foreach (MemberInfo prop in GetProps(t, listOnly))
            {
                object v = GetValue(prop, obj);
                if (v != null)
                    l.Add(v);
            }
            return l;
        }

        private static object GetValue(MemberInfo prop, object obj)
        {
            if (prop is PropertyInfo)
                return ((PropertyInfo) prop).GetValue(obj, null);
            if (prop is FieldInfo)
                return ((FieldInfo) prop).GetValue(obj);
            return null;
        }

        private static void SetValue(MemberInfo prop, object obj, object value)
        {
            if (prop is PropertyInfo)
                ((PropertyInfo) prop).SetValue(obj, value, null);
            if (prop is FieldInfo)
                ((FieldInfo) prop).SetValue(obj, value);
        }

        #endregion

        #region Safe-Thread Copy

        private static void Copy(object from, object to, MemberInfo p)
        {
            object fromValue = GetValue(p, from);
            if (IsList(p))
            {
                SetValue(p, to, CreateGenList(fromValue));
            }
            else
                SetValue(p, to, fromValue);
        }

        public static object CopyFrom(object from)
        {
            if (from is IEnumerable)
            {
                var l = (IEnumerable) from;
                IEnumerator en = l.GetEnumerator();
                // first list's element has properties-lists
                if (en.MoveNext() && GetProps(en.Current.GetType()).Count > 0)
                {
                    var gl = (IList) CreateGenList(from, en.Current.GetType(), false);
                    foreach (object el in l)
                        gl.Add(CopyFrom(el));
                    return gl;
                }
                return CreateGenList(from);
            }
            object c = Activator.CreateInstance(from.GetType());
            foreach (MemberInfo p in GetProps(from.GetType(), false))
            {
                Copy(from, c, p);
            }
            return c;
        }

        #endregion

        #region Lists 

        private static bool IsList(MemberInfo pr)
        {
            switch (pr.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo f = pr.DeclaringType.GetField(pr.Name);
                    return IsList(f.FieldType);
                case MemberTypes.Property:
                    PropertyInfo p = pr.DeclaringType.GetProperty(pr.Name);
                    return IsList(p.PropertyType);
            }
            return false;
        }

        private static bool IsList(Type t)
        {
            return typeof (IList).IsAssignableFrom(t);
        }

        private static object CreateGenList(object from)
        {
            Type[] pars = from.GetType().GetGenericArguments();
            return CreateGenList(from, pars.Length > 0 ? pars[0] : typeof (object), true);
        }

        private static object CreateGenList(object from, Type elType, bool init)
        {
            if (init)
                return Activator.CreateInstance(typeof (List<>).MakeGenericType(new[] {elType,}), from);
            return Activator.CreateInstance(typeof (List<>).MakeGenericType(new[] {elType,}));
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Exit(_LockedObj);
            _LockedObj = null;
        }

        #endregion

        public void Enter(object obj)
        {
            GetPropValues(obj.GetType(), true, obj).ForEach(Enter);
            Monitor.Enter(obj);
        }

        public void Exit(object obj)
        {
            GetPropValues(obj.GetType(), true, obj).ForEach(Exit);
            Monitor.Exit(obj);
        }
    }
}