using NUnit.Framework;
using Metsys.Bson.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metsys.Bson.Tests
{
    public class ConfigurationTests : IDisposable
    {        
		[Test]
        public void UsesAliasWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
            {
                t.UseAlias(p => p.Nint, "id");
                t.UseAlias(p => p.String, "str");
            });

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.AreEqual((byte)'i', result[5]);
            Assert.AreEqual((byte)'d', result[6]);
            Assert.AreEqual((byte)0, result[7]);

            Assert.AreEqual((byte)'s', result[13]);
            Assert.AreEqual((byte)'t', result[14]);
            Assert.AreEqual((byte)'r', result[15]);
            Assert.AreEqual((byte)0, result[16]);
        }

        [Test]
        public void MixesAliasAndNormalNameWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.UseAlias(p => p.Nint, "id"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.AreEqual((byte)'i', result[5]);
            Assert.AreEqual((byte)'d', result[6]);
            Assert.AreEqual((byte)0, result[7]);

            Assert.AreEqual((byte)'S', result[13]);
            Assert.AreEqual((byte)'t', result[14]);
            Assert.AreEqual((byte)'r', result[15]);
            Assert.AreEqual((byte)'i', result[16]);
            Assert.AreEqual((byte)'n', result[17]);
            Assert.AreEqual((byte)'g', result[18]);
            Assert.AreEqual((byte)0, result[19]);
        }
        
        [Test]
        public void UsesAliasWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
            {
                t.UseAlias(p => p.Nint, "id");
                t.UseAlias(p => p.String, "str");
            });

            var result = Serializer.Serialize(new { id = 43, str = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.AreEqual(43, o.Nint);
            Assert.AreEqual("abc", o.String);           
        }

        [Test]
        public void MixesAliasAndNormalNameWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.UseAlias(p => p.Nint, "id"));

            var result = Serializer.Serialize(new { id = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.AreEqual(43, o.Nint);
            Assert.AreEqual("abc", o.String);  
        }

        [Test]
        public void IgnoresPropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore(p => p.Nint));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.AreEqual(21, BitConverter.ToInt32(result, 0));
            Assert.AreEqual((byte)'S', result[5]);
            Assert.AreEqual((byte)'t', result[6]);
            Assert.AreEqual((byte)'r', result[7]);
            Assert.AreEqual((byte)'i', result[8]);
            Assert.AreEqual((byte)'n', result[9]);
            Assert.AreEqual((byte)'g', result[10]);
            Assert.AreEqual((byte)0, result[11]);            
        }

        [Test]
        public void IgnoresNamedPropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore("Nint"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.AreEqual(21, BitConverter.ToInt32(result, 0));
            Assert.AreEqual((byte)'S', result[5]);
            Assert.AreEqual((byte)'t', result[6]);
            Assert.AreEqual((byte)'r', result[7]);
            Assert.AreEqual((byte)'i', result[8]);
            Assert.AreEqual((byte)'n', result[9]);
            Assert.AreEqual((byte)'g', result[10]);
            Assert.AreEqual((byte)0, result[11]);
        }
        
        [Test]
        public void IgnoresMultiplePropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
                {
                    t.Ignore(p => p.Nint);
                    t.Ignore(p => p.String);
                });

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.AreEqual(5, BitConverter.ToInt32(result, 0));            
        }

        [Test]
        public void IgnoresPropertyWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore(p => p.Nint));

            var result = Serializer.Serialize(new { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);
            Assert.AreEqual(null, o.Nint);
            Assert.AreEqual("abc", o.String);
        }

        [Test]
        public void IgnoresNamedPropertyWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore("Nint"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);
            Assert.AreEqual(null, o.Nint);
            Assert.AreEqual("abc", o.String);
        }
        
        [Test]
        public void IgnoresMultiplePropertyWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
                {
                    t.Ignore(p => p.Nint);
                    t.Ignore(p => p.String);                
                });

            var result = Serializer.Serialize(new { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);
            Assert.AreEqual(null, o.Nint);
            Assert.AreEqual(null, o.String);
        }

        [Test]
        public void IgnoresPropertyWhenSerializingAndNull()
        {
            BsonConfiguration.ForType<Skinny>(t => t.IgnoreIfNull(p => p.Nint));

            var result = Serializer.Serialize(new Skinny { String = "abc" });
            Assert.AreEqual(21, BitConverter.ToInt32(result, 0));
            Assert.AreEqual((byte)'S', result[5]);
            Assert.AreEqual((byte)'t', result[6]);
            Assert.AreEqual((byte)'r', result[7]);
            Assert.AreEqual((byte)'i', result[8]);
            Assert.AreEqual((byte)'n', result[9]);
            Assert.AreEqual((byte)'g', result[10]);
            Assert.AreEqual((byte)0, result[11]);   
        }

		[TearDown]
        public void Dispose()
        {
            //this is all a horrible hack,someone fix it!
            typeof(BsonConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            typeof(TypeHelper).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, BsonConfiguration.Instance);
            typeof(TypeHelper).GetField("_cachedTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new Dictionary<Type, TypeHelper>());
        }
    }
}