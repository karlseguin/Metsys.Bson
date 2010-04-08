using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Metsys.Bson
{
    internal abstract class BaseWrapper
    {
        public static BaseWrapper Create(Type type, Type itemType, object existingContainer)
        {            
            var instance = CreateWrapperFromType(existingContainer == null ? type : existingContainer.GetType(), itemType);
            instance.SetContainer(existingContainer ?? instance.CreateContainer(type, itemType));
            return instance;            
        }

        private static BaseWrapper CreateWrapperFromType(Type type, Type itemType)
        {
            if (type.IsArray)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(ArrayWrapper<>).MakeGenericType(itemType));
            }

            var isCollection = false;
            var types = new List<Type>(type.GetInterfaces().Select(h => h.IsGenericType ? h.GetGenericTypeDefinition() : h));
            types.Insert(0, type.IsGenericType ? type.GetGenericTypeDefinition() : type);              
            foreach(var @interface in types)
            {
                if (typeof(IList<>).IsAssignableFrom(@interface) || typeof(IList).IsAssignableFrom(@interface))
                {
                    return new ListWrapper();
                }
                if (typeof(ICollection<>).IsAssignableFrom(@interface))
                {
                    isCollection = true;
                }
            }
            if (isCollection)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(itemType));
            }

            //a last-ditch pass
            foreach (var @interface in types)
            {
                if (typeof(IEnumerable<>).IsAssignableFrom(@interface) || typeof(IEnumerable).IsAssignableFrom(@interface))
                {
                    return new ListWrapper();
                }
            }
            throw new BsonException(string.Format("Collection of type {0} cannot be deserialized", type.FullName));
        }

        public abstract void Add(object value);
        public abstract object Collection { get; }

        protected abstract object CreateContainer(Type type, Type itemType);
        protected abstract void SetContainer(object container);        
    }      
}