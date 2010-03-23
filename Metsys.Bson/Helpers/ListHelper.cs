using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Metsys.Bson
{    
    internal static class ListHelper
    {
        public static Type GetListItemType(Type enumerableType)
        {
            if (enumerableType.IsArray)
            {
                return enumerableType.GetElementType();
            }

            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[0]
                : typeof(object);
        }

        public static Type GetDictionarKeyType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[0]
                : typeof(object);
        }

        public static Type GetDictionarValueType(Type enumerableType)
        {
            return enumerableType.IsGenericType
                ? enumerableType.GetGenericArguments()[1]
                : typeof(object);
        }

        public static Array ToArray(List<object> container, Type itemType)
        {
            var array = Array.CreateInstance(itemType, container.Count);
            Array.Copy(container.ToArray(), 0, array, 0, container.Count);
            return array;
        }

        public static IList CreateContainer(Type listType, Type listItemType, out bool isReadOnly)
        {
            isReadOnly = false;
            if (listType.IsArray)
            {
                return new List<object>();
            }

            if (listType.IsInterface)
            {
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));
            }

            if (listType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                return (IList)Activator.CreateInstance(listType);
            }

            if (typeof(ReadOnlyCollection<>).IsAssignableFrom(listType.GetGenericTypeDefinition()))
            {
                isReadOnly = true;
                return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(listItemType));
            }

            return new List<object>();
        }
       
        public static IDictionary CreateDictionary(Type dictionaryType, Type keyType, Type valueType)
        {
            if (dictionaryType.IsInterface)
            {
                return (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            }

            if (dictionaryType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null) != null)
            {
                return (IDictionary)Activator.CreateInstance(dictionaryType);
            }

            return new Dictionary<object, object>();
        }
    }
}