using System;

namespace Metsys.Bson.Configuration
{
    using System.Collections.Generic;

    public class BsonConfiguration
    {
        private readonly IDictionary<Type, IDictionary<string, string>> _aliasMap = new Dictionary<Type, IDictionary<string, string>>();
        private readonly IDictionary<Type, HashSet<string>> _ignored = new Dictionary<Type, HashSet<string>>();
        private readonly IDictionary<Type, HashSet<string>> _ignoredIfNull = new Dictionary<Type, HashSet<string>>();
        
        //not thread safe
        private static BsonConfiguration _instance;
        internal static BsonConfiguration Instance
        {
            get
            {
                if (_instance == null) { _instance = new BsonConfiguration(); }
                return _instance;
            }
        }
        
        private BsonConfiguration(){}

        public static void ForType<T>(Action<ITypeConfiguration<T>> action)
        {
            action(new TypeConfiguration<T>(Instance));
        }
        
        internal void AddMap<T>(string property, string alias)
        {
            var type = typeof (T);
            if (!_aliasMap.ContainsKey(type))
            {
                _aliasMap[type] = new Dictionary<string, string>();
            }
            _aliasMap[type][property] = alias;
        }        
        internal string AliasFor(Type type, string property)
        {            
            IDictionary<string, string> map;
            if (!_aliasMap.TryGetValue(type, out map))
            {
                return property;
            }
            return map.ContainsKey(property) ? map[property] : property;
        }

        public void AddIgnore<T>(string name)
        {
            var type = typeof(T);
            if (!_ignored.ContainsKey(type))
            {
                _ignored[type] = new HashSet<string>();
            }
            _ignored[type].Add(name);
        }
        public bool IsIgnored(Type type, string name)
        {
            HashSet<string> list;            
            return _ignored.TryGetValue(type, out list) && list.Contains(name);
        }

        public void AddIgnoreIfNull<T>(string name)
        {
            var type = typeof(T);
            if (!_ignoredIfNull.ContainsKey(type))
            {
                _ignoredIfNull[type] = new HashSet<string>();
            }
            _ignoredIfNull[type].Add(name);
        }
        public bool IsIgnoredIfNull(Type type, string name)
        {
            HashSet<string> list;
            return _ignoredIfNull.TryGetValue(type, out list) && list.Contains(name);
        }
    }
}