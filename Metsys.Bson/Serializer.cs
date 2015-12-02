using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Metsys.Bson
{    

    public class Serializer
    {
        private static readonly IDictionary<Type, Types> _typeMap = new Dictionary<Type, Types>
                                                                            {
                                                                                {typeof (int), Types.Int32},
                                                                                {typeof (long), Types.Int64},
                                                                                {typeof (bool), Types.Boolean},
                                                                                {typeof (string), Types.String},
                                                                                {typeof (double), Types.Double},
                                                                                {typeof (Guid), Types.Binary},
                                                                                {typeof (Regex), Types.Regex},
                                                                                {typeof (DateTime), Types.DateTime},
                                                                                {typeof (float), Types.Double},
                                                                                {typeof (byte[]), Types.Binary},
                                                                                {typeof (ObjectId), Types.ObjectId},
                                                                                {typeof (ScopedCode), Types.ScopedCode}
                                                                            };
 
        private readonly BinaryWriter _writer;
        private Document _current;

        public static byte[] Serialize<T>(T document)
        {
            var type = document.GetType();
			if (type.IsValueType ||
				( typeof(IEnumerable).IsAssignableFrom(type) && typeof(IDictionary).IsAssignableFrom(type) == false )
			)
            {
               throw new BsonException("Root type must be an object");
            }
            using (var ms = new MemoryStream(250))
            using (var writer = new BinaryWriter(ms))
            {
                new Serializer(writer).WriteDocument(document);
                return ms.ToArray();
            }
        }        
        private Serializer(BinaryWriter writer)
        {
            _writer = writer;
        }        
         
        private void NewDocument()
        {
            var old = _current;
            _current = new Document { Parent = old, Length = (int)_writer.BaseStream.Position, Digested = 4 };
            _writer.Write(0); // length placeholder
        }        
        private void EndDocument(bool includeEeo)
        {
            var old = _current;
            if (includeEeo)
            {
                Written(1);
                _writer.Write((byte)0);
            }
 
            _writer.Seek(_current.Length, SeekOrigin.Begin);
            _writer.Write(_current.Digested); // override the document length placeholder
            _writer.Seek(0, SeekOrigin.End); // back to the end
            _current = _current.Parent;
            if (_current != null)
            {
                Written(old.Digested);
            }
        }
 
        private void Written(int length)
        {
            _current.Digested += length;
        }
         
        private void WriteDocument(object document)
        {
            NewDocument();
            WriteObject(document); 
            EndDocument(true);
        }
 
        private void WriteObject(object document)
        {
			var asDictionary = document as IDictionary;
			if ( asDictionary != null )
			{
				Write( asDictionary );
				return;
			}

            var typeHelper = TypeHelper.GetHelperForType(document.GetType());
            foreach (var property in typeHelper.GetProperties())
            {
                if (property.Ignored) { continue; }
                var name = property.Name; 
                var value = property.Getter(document);
                if (value == null && property.IgnoredIfNull)
                {
                    continue;
                }
                SerializeMember(name, value);
            }          
        }
 
        private void SerializeMember(string name, object value)
        {
            if (value == null)
            {
                Write(Types.Null);
                WriteName(name);
                return;
            }
 
            var type = value.GetType();
            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }
 
            Types storageType;
            if (!_typeMap.TryGetValue(type, out storageType))
            {
                // this isn't a simple type;
                Write(name, value);
                return;
            }
 
            Write(storageType);
            WriteName(name);
            switch (storageType)
            {
                case Types.Int32:
                    Written(4);
                    _writer.Write((int)value);
                    return;
                case Types.Int64:
                    Written(8);
                    _writer.Write((long)value);
                    return;
                case Types.String:
                    Write((string)value);
                    return;
                case Types.Double:
                    Written(8);
                    if (value is float)
                    {
                        _writer.Write(Convert.ToDouble((float)value));
                    }
                    else
                    {
                        _writer.Write((double)value);
                    }
 
                    return;
                case Types.Boolean:
                    Written(1);
                    _writer.Write((bool)value ? (byte)1 : (byte)0);
                    return;
                case Types.DateTime:
                    Written(8);
					_writer.Write((long)((DateTime)value).ToUniversalTime().Subtract(Helper.Epoch).TotalMilliseconds);
                    return;
                case Types.Binary:
                    WriteBinnary(value);
                    return;
                case Types.ScopedCode:
                    Write((ScopedCode)value);
                    return;
                case Types.ObjectId:
                    Written(((ObjectId)value).Value.Length);
                    _writer.Write(((ObjectId)value).Value);
                    return;
                case Types.Regex:
                    Write((Regex)value);
                    break;
            }
        }
 
        private void Write(string name, object value)
        {
            if (value is IDictionary)
            {
                Write(Types.Object);
                WriteName(name);
                NewDocument();
                Write((IDictionary)value);
                EndDocument(true);
            }
            else if (value is IEnumerable)
            {
                Write(Types.Array);
                WriteName(name);
                NewDocument();
                Write((IEnumerable)value);
                EndDocument(true);
            }       
            else
            {
                Write(Types.Object);
                WriteName(name);
                WriteDocument(value); // Write manages new/end document
            }
        }
 
        private void Write(IEnumerable enumerable)
        {
            var index = 0;
            foreach (var value in enumerable)
            {
                SerializeMember((index++).ToString(), value);
            }
        }
 
        private void Write(IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                SerializeMember((string)key, dictionary[key]);
            }
        }
 
        private void WriteBinnary(object value)
        {
            if (value is byte[])
            {
                var bytes = (byte[])value;
                var length = bytes.Length;
                _writer.Write(length + 4);
                _writer.Write((byte)2);
                _writer.Write(length);
                _writer.Write(bytes);
                Written(9 + length);
            }
            else if (value is Guid)
            {
                var guid = (Guid)value;
                var bytes = guid.ToByteArray();
                _writer.Write(bytes.Length);
                _writer.Write((byte)3);
                _writer.Write(bytes);
                Written(5 + bytes.Length);
            }
        }
 
        private void Write(Types type)
        {
            _writer.Write((byte)type);
            Written(1);
        }
 
        private void WriteName(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 1);
        }
 
        private void Write(string name)
        {
            var bytes = Encoding.UTF8.GetBytes(name);
            _writer.Write(bytes.Length + 1);
            _writer.Write(bytes);
            _writer.Write((byte)0);
            Written(bytes.Length + 5); // stringLength + length + null byte
        }
 
        private void Write(Regex regex)
        {
            WriteName(regex.ToString());
 
            var options = string.Empty;
            if ((regex.Options & RegexOptions.ECMAScript) == RegexOptions.ECMAScript)
            {
                options = string.Concat(options, 'e');
            }
 
            if ((regex.Options & RegexOptions.IgnoreCase) == RegexOptions.IgnoreCase)
            {
                options = string.Concat(options, 'i');
            }
 
            if ((regex.Options & RegexOptions.CultureInvariant) == RegexOptions.CultureInvariant)
            {
                options = string.Concat(options, 'l');
            }
 
            if ((regex.Options & RegexOptions.Multiline) == RegexOptions.Multiline)
            {
                options = string.Concat(options, 'm');
            }
 
            if ((regex.Options & RegexOptions.Singleline) == RegexOptions.Singleline)
            {
                options = string.Concat(options, 's');
            }
 
            options = string.Concat(options, 'u'); // all .net regex are unicode regex, therefore:
            if ((regex.Options & RegexOptions.IgnorePatternWhitespace) == RegexOptions.IgnorePatternWhitespace)
            {
                options = string.Concat(options, 'w');
            }
 
            if ((regex.Options & RegexOptions.ExplicitCapture) == RegexOptions.ExplicitCapture)
            {
                options = string.Concat(options, 'x');
            }
 
            WriteName(options);
        }
 
        private void Write(ScopedCode value)
        {
            NewDocument();
            Write(value.CodeString);
            WriteDocument(value.Scope);
            EndDocument(false);
        }
    }
}