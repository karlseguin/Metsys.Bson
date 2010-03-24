using System;
using System.Reflection;
using Metsys.Bson.Configuration;

namespace Metsys.Bson
{
    internal class MagicProperty
    {
        private readonly PropertyInfo _property;          
        private readonly string _name;
        private readonly bool _ignored;
        public readonly bool _ignoredIfNull;

        public Type Type
        {
            get { return _property.PropertyType; }
        }
        public string Name
        {
            get { return _name; }
        }
        public bool Ignored
        {
            get { return _ignored; }
        }
        public bool IgnoredIfNull
        {
            get { return _ignoredIfNull; }
        }

        public Action<object, object> Setter { get; private set; }

        public Func<object, object> Getter { get; private set; }
        
        public MagicProperty(PropertyInfo property, string name, bool ignored, bool ignoredIfNull)
        {
            _property = property;
            _name = name;
            _ignored = ignored;
            _ignoredIfNull = ignoredIfNull;
            Getter = CreateGetterMethod(property);
            Setter = CreateSetterMethod(property);
        }
               
        private static Action<object, object> CreateSetterMethod(PropertyInfo property)
        {
            var genericHelper = typeof(MagicProperty).GetMethod("SetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Action<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }
               
        private static Func<object, object> CreateGetterMethod(PropertyInfo property)
        {
            var genericHelper = typeof(MagicProperty).GetMethod("GetterMethod", BindingFlags.Static | BindingFlags.NonPublic);
            var constructedHelper = genericHelper.MakeGenericMethod(property.DeclaringType, property.PropertyType);
            return (Func<object, object>)constructedHelper.Invoke(null, new object[] { property });
        }

        //called via reflection       
        private static Action<object, object> SetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetSetMethod(true);
            if (m == null) { return null; } //no setter
            var func = (Action<TTarget, TParam>)Delegate.CreateDelegate(typeof(Action<TTarget, TParam>), m);
            return (target, param) => func((TTarget)target, (TParam)param);
        }

        //called via reflection
        private static Func<object, object> GetterMethod<TTarget, TParam>(PropertyInfo method) where TTarget : class
        {
            var m = method.GetGetMethod(true);
            var func = (Func<TTarget, TParam>)Delegate.CreateDelegate(typeof(Func<TTarget, TParam>), m);
            return target => func((TTarget)target);
        }
    }
}
