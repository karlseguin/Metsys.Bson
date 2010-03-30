using System;
using System.Collections;
using System.Collections.Generic;

namespace Metsys.Bson
{
    internal abstract class BaseWrapper
    {
        public static BaseWrapper Create(Type type, Type itemType, object existingContainer)
        {            
            var instance = CreateWrapperFromType(existingContainer == null ? type : existingContainer.GetType(), itemType);
            instance.SetContainer(existingContainer);
            return instance;            
        }

        private static BaseWrapper CreateWrapperFromType(Type type, Type itemType)
        {
            if (type.IsArray)
            {
                return (BaseWrapper)Activator.CreateInstance(typeof(ArrayWrapper<>).MakeGenericType(itemType));
            }
            
            foreach(var @interface in type.GetInterfaces())
            {
                var generic = @interface.GetGenericTypeDefinition();
                if (typeof(IList).IsAssignableFrom(generic))
                {
                    return new ListWrapper();
                }
                if (typeof(ICollection<>).IsAssignableFrom(generic))
                {
                    return (BaseWrapper) Activator.CreateInstance(typeof(CollectionWrapper<>).MakeGenericType(itemType));                    
                }
            }
            throw new BsonException(string.Format("Collection of type {0} cannot be deserialized", type.FullName));
        }

        public abstract void Add(object value);
        protected abstract void SetContainer(object container);      
        public abstract object Collection{ get; }  
    }      
}