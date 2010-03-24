using System;

namespace Metsys.Bson.Configuration
{
    using System.Collections.Generic;

    public class BsonConfiguration
    {
        private readonly IDictionary<Type, IDictionary<string, string>> _aliasMap = new Dictionary<Type, IDictionary<string, string>>();
        
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
    }
}