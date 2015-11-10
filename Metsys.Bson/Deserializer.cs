using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
        
namespace Metsys.Bson
{
    public class Deserializer
    {        
		public class Options
		{
			public bool LongIntegers { get; set; }
		}

        private readonly static IDictionary<Types, Type> _typeMap = new Dictionary<Types, Type>
        {
			{Types.Int32, typeof(int)},
			{Types.Int64, typeof (long)},
			{Types.Boolean, typeof (bool)},
			{Types.String, typeof (string)},
			{Types.Double, typeof(double)},
			{Types.Binary, typeof (byte[])},
			{Types.Regex, typeof (Regex)},
			{Types.DateTime, typeof (DateTime)},
			{Types.ObjectId, typeof(ObjectId)},
			{Types.Array, typeof(List<object>)},
			{Types.Object, typeof(Dictionary<string, object>)},
        };
        private readonly BinaryReader _reader;
        private Document _current;

        private Deserializer(BinaryReader reader)
        {
            _reader = reader;
        }
        
        public static T Deserialize<T>(byte[] objectData, Options options = null) where T : class
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(objectData, 0, objectData.Length);
                ms.Position = 0;
				return Deserialize<T>(new BinaryReader(ms), options ?? new Options());
            }
        }
        
		private static T Deserialize<T>(BinaryReader stream, Options options)
        {
            return new Deserializer(stream).Read<T>(options);
        }

		private T Read<T>(Options options)
        {
            NewDocument(_reader.ReadInt32());
			var @object = (T)DeserializeValue(typeof(T), Types.Object, options);
            return @object;
        }

        private void Read(int read)
        {
            _current.Digested += read;
        }

        private bool IsDone()
        {
            var isDone = _current.Digested + 1 == _current.Length;
            if (isDone)
            {
                _reader.ReadByte(); // EOO
                var old = _current;
                _current = old.Parent;
                if (_current != null) { Read(old.Length); }
            }
            return isDone;
        }
        
        private void NewDocument(int length)
        {
            var old = _current;
            _current = new Document { Length = length, Parent = old, Digested = 4 };
        }

		private object DeserializeValue(Type type, Types storedType, Options options)
        {
            return DeserializeValue(type, storedType, null, options);
        }

		private object DeserializeValue(Type type, Types storedType, object container, Options options)
        {
            if (storedType == Types.Null)
            {
                return null;
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }
            if (type == typeof(string))
            {
                return ReadString();
            }
            if (type == typeof(int))
            {
				var val = ReadInt(storedType);
				return options.LongIntegers ? (object)(long) val : (object) val;
            }
            if (type.IsEnum)
            {
                return ReadEnum(type, storedType);
            }
            if (type == typeof(float))
            {
                Read(8);
				return (float) _reader.ReadDouble();
            }
            if (storedType == Types.Binary)
            {
                return ReadBinary();
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return ReadList(type, container, options);
            }
            if (type == typeof(bool))
            {
                Read(1);
                return _reader.ReadBoolean();
            }
            if (type == typeof(DateTime))
            {
                return Helper.Epoch.AddMilliseconds(ReadLong(Types.Int64));
            }
            if (type == typeof(ObjectId))
            {
                Read(12);
                return new ObjectId(_reader.ReadBytes(12));
            }
            if (type == typeof(long))
            {
                return ReadLong(storedType);
            }
            if (type == typeof(double))
            {
                Read(8);
                return _reader.ReadDouble();
            }
            if (type == typeof(Regex))
            {
                return ReadRegularExpression();
            }
            if (type == typeof(ScopedCode))
            {
                return ReadScopedCode(options);
            }
            return ReadObject(type, options);
        }

		private object ReadObject(Type type, Options options)
        {
            var instance = Activator.CreateInstance(type, true);
            var typeHelper = TypeHelper.GetHelperForType(type);
            while (true)
            {
                var storageType = ReadType();
                var name = ReadName();                
                var isNull = false;
                if (storageType == Types.Object)
                {
                    var length = _reader.ReadInt32();
                    if (length == 5)
                    {
                        _reader.ReadByte(); //eoo
                        Read(5);
                        isNull = true;
                    }
                    else
                    {
                        NewDocument(length);
                    }
                }
                object container = null;
                var property = typeHelper.FindProperty(name);
                var propertyType = property != null ? property.Type : _typeMap.ContainsKey(storageType) ? _typeMap[storageType] : typeof(object);
                if (property == null && typeHelper.Expando == null)
                {
                    throw new BsonException(string.Format("Deserialization failed: type {0} does not have a property named {1}", type.FullName, name));
                }                
                if (property != null && property.Setter == null)
                {
                    container = property.Getter(instance);                    
                }
                var value = isNull ? null : DeserializeValue(propertyType, storageType, container, options);
                if (property == null)
                {
                    ((IDictionary<string, object>)typeHelper.Expando.Getter(instance))[name] = value;
                }
                else if (container == null && value != null && !property.Ignored)
                {
                    property.Setter(instance, value);
                }
                if (IsDone())
                {
                    break;
                }
            }
            return instance;
        }

		private object ReadList(Type listType, object existingContainer, Options options)
        {
            if (IsDictionary(listType))
            {
                return ReadDictionary(listType, existingContainer, options);
            }

            NewDocument(_reader.ReadInt32());            
            var itemType = ListHelper.GetListItemType(listType);
            var isObject = typeof(object) == itemType;
            var wrapper = BaseWrapper.Create(listType, itemType, existingContainer);
                      
            while (!IsDone())
            {
                var storageType = ReadType();
                ReadName();
                if (storageType == Types.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var specificItemType = isObject ? _typeMap[storageType] : itemType;
                var value = DeserializeValue(specificItemType, storageType, options);
                wrapper.Add(value);
            }
            return wrapper.Collection;
        }
        
        private static bool IsDictionary(Type type)
        {
            var types = new List<Type>(type.GetInterfaces());
            types.Insert(0, type);
            foreach (var interfaceType in types)
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    return true;
                }
            }
            return false; 
        }

		private object ReadDictionary(Type listType, object existingContainer, Options options)
        {
            var valueType = ListHelper.GetDictionarValueType(listType);
            var isObject = typeof (object) == valueType;
            var container = existingContainer == null ? ListHelper.CreateDictionary(listType, ListHelper.GetDictionarKeyType(listType), valueType) : (IDictionary)existingContainer;

            while (!IsDone())
            {
                var storageType = ReadType();

                var key = ReadName();
                if (storageType == Types.Object)
                {
                    NewDocument(_reader.ReadInt32());
                }
                var specificItemType = isObject ? _typeMap[storageType] : valueType;
                var value = DeserializeValue(specificItemType, storageType, options);
                container.Add(key, value);
            }
            return container;
        }

        private object ReadBinary()
        {
            var length = _reader.ReadInt32();
            var subType = _reader.ReadByte();
            Read(5 + length);
            if (subType == 2)
            {
                return _reader.ReadBytes(_reader.ReadInt32());
            }
            if (subType == 3)
            {
                return new Guid(_reader.ReadBytes(length));
            }
            throw new BsonException("No support for binary type: " + subType);
        }

        private string ReadName()
        {
            var buffer = new List<byte>(128); //todo: use a pool to prevent fragmentation
            byte b;
            while ((b = _reader.ReadByte()) > 0)
            {
                buffer.Add(b);
            }
            Read(buffer.Count + 1);
            return Encoding.UTF8.GetString(buffer.ToArray());
        }

        private string ReadString()
        {
            var length = _reader.ReadInt32();
            var buffer = _reader.ReadBytes(length - 1); //todo: again, look at fragementation prevention
            _reader.ReadByte(); //null;
            Read(4 + length);

            return Encoding.UTF8.GetString(buffer);
        }

        private int ReadInt(Types storedType)
        {
            switch (storedType)
            {
                case Types.Int32:
                    Read(4);
                    return _reader.ReadInt32();
                case Types.Int64:
                    Read(8);
                    return (int)_reader.ReadInt64();
                case Types.Double:
                    Read(8);
                    return (int)_reader.ReadDouble();
                default:
                    throw new BsonException("Could not create an int from " + storedType);
            }
        }

        private long ReadLong(Types storedType)
        {
            switch (storedType)
            {
                case Types.Int32:
                    Read(4);
                    return _reader.ReadInt32();
                case Types.Int64:
                    Read(8);
                    return _reader.ReadInt64();
                case Types.Double:
                    Read(8);
                    return (long)_reader.ReadDouble();
                default:
                    throw new BsonException("Could not create an int64 from " + storedType);
            }
        }

        private object ReadEnum(Type type, Types storedType)
        {
            if (storedType == Types.Int64)
            {
                return Enum.Parse(type, ReadLong(storedType).ToString(), false);
            }
            return Enum.Parse(type, ReadInt(storedType).ToString(), false);
        }

        private object ReadRegularExpression()
        {
            var pattern = ReadName();
            var optionsString = ReadName();

            var options = RegexOptions.None;
            if (optionsString.Contains("e")) options = options | RegexOptions.ECMAScript;
            if (optionsString.Contains("i")) options = options | RegexOptions.IgnoreCase;
            if (optionsString.Contains("l")) options = options | RegexOptions.CultureInvariant;
            if (optionsString.Contains("m")) options = options | RegexOptions.Multiline;
            if (optionsString.Contains("s")) options = options | RegexOptions.Singleline;
            if (optionsString.Contains("w")) options = options | RegexOptions.IgnorePatternWhitespace;
            if (optionsString.Contains("x")) options = options | RegexOptions.ExplicitCapture;

            return new Regex(pattern, options);
        }

        private Types ReadType()
        {
            Read(1);
            return (Types)_reader.ReadByte();
        }

		private ScopedCode ReadScopedCode(Options options)
        {
            _reader.ReadInt32(); //length
            Read(4);
            var name = ReadString();
            NewDocument(_reader.ReadInt32());
            return new ScopedCode { CodeString = name, Scope = DeserializeValue(typeof(object), Types.Object, options) };
        }
    }
}