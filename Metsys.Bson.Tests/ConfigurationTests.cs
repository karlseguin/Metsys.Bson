using Xunit;
using Metsys.Bson.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metsys.Bson.Tests
{
    public class ConfigurationTests : IDisposable
    {        
        [Fact]
        public void UsesAliasWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
            {
                t.UseAlias(p => p.Nint, "id");
                t.UseAlias(p => p.String, "str");
            });

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.Equal((byte)'i', result[5]);
            Assert.Equal((byte)'d', result[6]);
            Assert.Equal((byte)0, result[7]);

            Assert.Equal((byte)'s', result[13]);
            Assert.Equal((byte)'t', result[14]);
            Assert.Equal((byte)'r', result[15]);
            Assert.Equal((byte)0, result[16]);
        }

        [Fact]
        public void MixesAliasAndNormalNameWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.UseAlias(p => p.Nint, "id"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.Equal((byte)'i', result[5]);
            Assert.Equal((byte)'d', result[6]);
            Assert.Equal((byte)0, result[7]);

            Assert.Equal((byte)'S', result[13]);
            Assert.Equal((byte)'t', result[14]);
            Assert.Equal((byte)'r', result[15]);
            Assert.Equal((byte)'i', result[16]);
            Assert.Equal((byte)'n', result[17]);
            Assert.Equal((byte)'g', result[18]);
            Assert.Equal((byte)0, result[19]);
        }
        
        [Fact]
        public void UsesAliasWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
            {
                t.UseAlias(p => p.Nint, "id");
                t.UseAlias(p => p.String, "str");
            });

            var result = Serializer.Serialize(new { id = 43, str = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.Equal(43, o.Nint);
            Assert.Equal("abc", o.String);           
        }

        [Fact]
        public void MixesAliasAndNormalNameWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.UseAlias(p => p.Nint, "id"));

            var result = Serializer.Serialize(new { id = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.Equal(43, o.Nint);
            Assert.Equal("abc", o.String);  
        }

        [Fact]
        public void IgnoresPropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore(p => p.Nint));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.Equal(21, BitConverter.ToInt32(result, 0));
            Assert.Equal((byte)'S', result[5]);
            Assert.Equal((byte)'t', result[6]);
            Assert.Equal((byte)'r', result[7]);
            Assert.Equal((byte)'i', result[8]);
            Assert.Equal((byte)'n', result[9]);
            Assert.Equal((byte)'g', result[10]);
            Assert.Equal((byte)0, result[11]);            
        }

        [Fact]
        public void IgnoresNamedPropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore("Nint"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.Equal(21, BitConverter.ToInt32(result, 0));
            Assert.Equal((byte)'S', result[5]);
            Assert.Equal((byte)'t', result[6]);
            Assert.Equal((byte)'r', result[7]);
            Assert.Equal((byte)'i', result[8]);
            Assert.Equal((byte)'n', result[9]);
            Assert.Equal((byte)'g', result[10]);
            Assert.Equal((byte)0, result[11]);
        }
        
        [Fact]
        public void IgnoresMultiplePropertyWhenSerializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
                {
                    t.Ignore(p => p.Nint);
                    t.Ignore(p => p.String);
                });

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            Assert.Equal(5, BitConverter.ToInt32(result, 0));            
        }

        [Fact]
        public void IgnoresPropertyWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.Ignore(p => p.Nint));

            var result = Serializer.Serialize(new { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);
            Assert.Equal(null, o.Nint);
            Assert.Equal("abc", o.String);
        }
        
        [Fact]
        public void IgnoresMultiplePropertyWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t =>
                {
                    t.Ignore(p => p.Nint);
                    t.Ignore(p => p.String);                
                });

            var result = Serializer.Serialize(new { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);
            Assert.Equal(null, o.Nint);
            Assert.Equal(null, o.String);
        }

        [Fact]
        public void IgnoresPropertyWhenSerializingAndNull()
        {
            BsonConfiguration.ForType<Skinny>(t => t.IgnoreIfNull(p => p.Nint));

            var result = Serializer.Serialize(new Skinny { String = "abc" });
            Assert.Equal(21, BitConverter.ToInt32(result, 0));
            Assert.Equal((byte)'S', result[5]);
            Assert.Equal((byte)'t', result[6]);
            Assert.Equal((byte)'r', result[7]);
            Assert.Equal((byte)'i', result[8]);
            Assert.Equal((byte)'n', result[9]);
            Assert.Equal((byte)'g', result[10]);
            Assert.Equal((byte)0, result[11]);   
        }

        public void Dispose()
        {
            //this is all a horrible hack,someone fix it!
            typeof(BsonConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            typeof(TypeHelper).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, BsonConfiguration.Instance);
            typeof(TypeHelper).GetField("_cachedTypeLookup", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new Dictionary<Type, TypeHelper>());
        }
    }
}