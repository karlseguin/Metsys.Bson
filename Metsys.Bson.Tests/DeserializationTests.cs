using Xunit;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Metsys.Bson.Tests
{
    public class DeserializationTests
    {
        [Fact]
        public void DeserializesAnInteger()
        {
            var input = Serializer.Serialize(new {Int = 72});
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(72, o.Int);
        }
        [Fact]
        public void DeserializesALong()
        {
            var input = Serializer.Serialize(new { Long = 993l });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(993, o.Long);
        }
        [Fact]
        public void DeserializesAFloat()
        {
            var input = Serializer.Serialize(new { Float = 1003.324f });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(1003.324f, o.Float);
        }
        [Fact]
        public void DeserializesADouble()
        {
            var input = Serializer.Serialize(new { Double = 1003.324 });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(1003.324, o.Double);
        }
        [Fact]
        public void DeserializesAString()
        {
            var input = Serializer.Serialize(new { String = "Its Over 9000!" });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal("Its Over 9000!", o.String);
        }
        [Fact]
        public void DeserializesAGuid()
        {
            var guid = Guid.NewGuid();
            var input = Serializer.Serialize(new { Guid = guid });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(guid, o.Guid);
        }
        [Fact]
        public void DeserializesAByteArray()
        {
            var array = new byte[] {1, 2, 3, 100, 94};
            var input = Serializer.Serialize(new { ByteArray = array });
            var o = Deserializer.Deserialize<Fatty>(input);            
            Assert.Equal(array, o.ByteArray);
        }
        [Fact]
        public void DeserializesADateTime()
        {
            var date = new DateTime(2001, 4, 8, 10, 43, 23, 104);
            var input = Serializer.Serialize(new { DateTime = date });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(date, o.DateTime);
        }
        [Fact]
        public void DeserializesARegex()
        {
            var regex = new Regex("its over (\\d+?)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var input = Serializer.Serialize(new { Regex = regex });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(regex.ToString(), o.Regex.ToString());
            Assert.Equal(regex.Options, o.Regex.Options);
        }
        [Fact]
        public void DeserializesAnArray()
        {
            var array = new object[] { 1, "a" };
            var input = Serializer.Serialize(new { Array = array });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(array, o.Array);
        }
        [Fact]
        public void DeserializesAList()
        {
            var list = new List<string> {"a", "ouch"};
            var input = Serializer.Serialize(new { List = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(list, o.List);
        }
        [Fact]
        public void DeserializesAHashSet()
        {
            var list = new HashSet<string> { "a", "ouch" };
            var input = Serializer.Serialize(new { HashSet = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(list, o.HashSet);             
        }
        [Fact]
        public void DeserializesAnIDictionary()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", "tWo" } };
            var input = Serializer.Serialize(new { IDictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(1, o.IDictionary["fiRst"]);
            Assert.Equal("tWo", o.IDictionary["second"]);
        }
        [Fact]
        public void DeserializesADictionary()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", "tWo" } };
            var input = Serializer.Serialize(new { Dictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(1, o.Dictionary["fiRst"]);
            Assert.Equal("tWo", o.Dictionary["second"]);
        }
        [Fact]
        public void DeserializesToAListWithNoSetter()
        {
            var list = new List<int> { 1, 9393 };
            var input = Serializer.Serialize(new { SetterLessList = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(list, o.SetterLessList);
        }
        [Fact]
        public void DeserializesToAHahsSetWithNoSetter()
        {
            var list = new HashSet<int> { 1, 9393 };
            var input = Serializer.Serialize(new { SetterlessHashSet = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(list, o.SetterlessHashSet);
        }
        [Fact]
        public void DeserializesAnIDictionaryWithNoSetter()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", 2 } };
            var input = Serializer.Serialize(new { SetterLessIDictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.Equal(1, o.SetterLessIDictionary["fiRst"]);
            Assert.Equal(2, o.SetterLessIDictionary["second"]);
        }
        [Fact]
        public void DeserializesToAClassWithPrivateConstructor()
        {
            var input = Serializer.Serialize(new { Key = "the key" });
            var o = Deserializer.Deserialize<Private>(input);
            Assert.Equal("the key", o.Key); 
        }
        [Fact]
        public void ThrowsExceptionWhenNoDefaultConstructorExists()
        {
            var input = Serializer.Serialize(new { Key = "the key" });
            Assert.Throws<MissingMethodException>(() => Deserializer.Deserialize<Impossible>(input));
        }
        [Fact]
        public void DeserializesUnknownValuesToExpando()
        {
            var input = Serializer.Serialize(new { Key = "the key", Another = 4, Final = "four" });
            var o = Deserializer.Deserialize<Expandotator>(input);
            Assert.Equal("the key", o.Key);
            Assert.Equal(4, o.Expando["Another"]);
            Assert.Equal("four", o.Expando["Final"]);
            Assert.Equal(2, o.Expando.Count);
        }
        [Fact]
        public void ThrowsExceptionForUnknownPropertyWithoutExpando()
        {
            var input = Serializer.Serialize(new { Key = "the key", Another = 4, Final = "four" });
            Assert.Throws<BsonException>(() => Deserializer.Deserialize<Skinny>(input));                        
        }
        
    }
}