using System.Linq;
using NUnit.Framework;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Metsys.Bson.Tests
{
    public class DeserializationTests
    {
        [Test]
        public void DeserializesAnInteger()
        {
            var input = Serializer.Serialize(new {Int = 72});
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(72, o.Int);
        }
        [Test]
        public void DeserializesALong()
        {
            var input = Serializer.Serialize(new { Long = 993l });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(993, o.Long);
        }
        [Test]
        public void DeserializesAFloat()
        {
            var input = Serializer.Serialize(new { Float = 1003.324f });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(1003.324f, o.Float);
        }
        [Test]
        public void DeserializesADouble()
        {
            var input = Serializer.Serialize(new { Double = 1003.324 });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(1003.324, o.Double);
        }
        [Test]
        public void DeserializesAString()
        {
            var input = Serializer.Serialize(new { String = "Its Over 9000!" });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual("Its Over 9000!", o.String);
        }
        [Test]
        public void DeserializesAGuid()
        {
            var guid = Guid.NewGuid();
            var input = Serializer.Serialize(new { Guid = guid });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(guid, o.Guid);
        }
        [Test]
        public void DeserializesAByteArray()
        {
            var array = new byte[] {1, 2, 3, 100, 94};
            var input = Serializer.Serialize(new { ByteArray = array });
            var o = Deserializer.Deserialize<Fatty>(input);            
            Assert.AreEqual(array, o.ByteArray);
        }
        [Test]
        public void DeserializesADateTime()
        {
            var date = new DateTime(2001, 4, 8, 10, 43, 23, 104);
            var input = Serializer.Serialize(new { DateTime = date });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(date, o.DateTime);
        }
        [Test]
        public void DeserializesARegex()
        {
            var regex = new Regex("its over (\\d+?)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var input = Serializer.Serialize(new { Regex = regex });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(regex.ToString(), o.Regex.ToString());
            Assert.AreEqual(regex.Options, o.Regex.Options);
        }
        [Test]
        public void DeserializesAnArray()
        {
            var array = new object[] { 1, "a" };
            var input = Serializer.Serialize(new { Array = array });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(array, o.Array);
        }
        [Test]
        public void DeserializesAList()
        {
            var list = new List<string> {"a", "ouch"};
            var input = Serializer.Serialize(new { List = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(list, o.List);
        }
        [Test]
        public void DeserializesAHashSet()
        {
            var list = new HashSet<string> { "a", "ouch" };
            var input = Serializer.Serialize(new { HashSet = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(list, o.HashSet);             
        }
        [Test]
        public void DeserializesAnIDictionary()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", "tWo" } };
            var input = Serializer.Serialize(new { IDictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(1, o.IDictionary["fiRst"]);
            Assert.AreEqual("tWo", o.IDictionary["second"]);
        }
        [Test]
        public void DeserializesADictionary()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", "tWo" } };
            var input = Serializer.Serialize(new { Dictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(1, o.Dictionary["fiRst"]);
            Assert.AreEqual("tWo", o.Dictionary["second"]);
        }
        [Test]
        public void DeserializesToAListWithNoSetter()
        {
            var list = new List<int> { 1, 9393 };
            var input = Serializer.Serialize(new { SetterLessList = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(list, o.SetterLessList);
        }
        [Test]
        public void DeserializesToAHahsSetWithNoSetter()
        {
            var list = new HashSet<int> { 1, 9393 };
            var input = Serializer.Serialize(new { SetterlessHashSet = list });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(list, o.SetterlessHashSet);
        }
        [Test]
        public void DeserializesAnIDictionaryWithNoSetter()
        {
            var dictionary = new Dictionary<string, object> { { "fiRst", 1 }, { "second", 2 } };
            var input = Serializer.Serialize(new { SetterLessIDictionary = dictionary });
            var o = Deserializer.Deserialize<Fatty>(input);
            Assert.AreEqual(1, o.SetterLessIDictionary["fiRst"]);
            Assert.AreEqual(2, o.SetterLessIDictionary["second"]);
        }
        [Test]
        public void DeserializesToAClassWithPrivateConstructor()
        {
            var input = Serializer.Serialize(new { Key = "the key" });
            var o = Deserializer.Deserialize<Private>(input);
            Assert.AreEqual("the key", o.Key); 
        }
        [Test]
        public void ThrowsExceptionWhenNoDefaultConstructorExists()
        {
            var input = Serializer.Serialize(new { Key = "the key" });
            Assert.Throws<MissingMethodException>(() => Deserializer.Deserialize<Impossible>(input));
        }
        [Test]
        public void DeserializesUnknownValuesToExpando()
        {
            var input = Serializer.Serialize(new { Key = "the key", Another = 4, Final = "four" });
            var o = Deserializer.Deserialize<Expandotator>(input);
            Assert.AreEqual("the key", o.Key);
            Assert.AreEqual(4, o.Expando["Another"]);
            Assert.AreEqual("four", o.Expando["Final"]);
            Assert.AreEqual(2, o.Expando.Count);
        }
        [Test]
        public void ThrowsExceptionForUnknownPropertyWithoutExpando()
        {
            var input = Serializer.Serialize(new { Key = "the key", Another = 4, Final = "four" });
            Assert.Throws<BsonException>(() => Deserializer.Deserialize<Skinny>(input));                        
        }

        [Test]
        public void SerializationOfIEnumerableTIsNotLossy()
        {
            var gto = new SpecialEnumerable { AnIEnumerable = new List<Skinny> { new Skinny(), new Skinny() } };
            var input = Serializer.Serialize(gto);
            var o = Deserializer.Deserialize<SpecialEnumerable>(input);
            Assert.AreEqual(2, o.AnIEnumerable.Count());
        }  
        
    }
}