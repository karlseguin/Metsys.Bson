using Xunit;
using Metsys.Bson.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metsys.Bson.Tests
{
    public class ConfigurationTests
    {
        public ConfigurationTests()
        {
            //this is all a horrible hack,someone fix it!
            typeof(BsonConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            typeof(TypeHelper).GetField("_configuration", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, BsonConfiguration.Instance);
            typeof(TypeHelper).GetField("_cachedTypeLookup",  BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, new Dictionary<Type, TypeHelper>());
        }
        
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

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.Equal(43, o.Nint);
            Assert.Equal("abc", o.String);           
        }

        [Fact]
        public void MixesAliasAndNormalNameWhenDeserializing()
        {
            BsonConfiguration.ForType<Skinny>(t => t.UseAlias(p => p.Nint, "id"));

            var result = Serializer.Serialize(new Skinny { Nint = 43, String = "abc" });
            var o = Deserializer.Deserialize<Skinny>(result);

            Assert.Equal(43, o.Nint);
            Assert.Equal("abc", o.String);  
        }
    }
}