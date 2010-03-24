using Xunit;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Metsys.Bson.Tests
{
    public class SerializationTests
    {
        [Fact]
        public void SerializesAProperty()
        {
            var result = Serializer.Serialize(new {Name = 4});
            Assert.Equal((byte)'N', result[5]);
            Assert.Equal((byte)'a', result[6]);
            Assert.Equal((byte)'m', result[7]);
            Assert.Equal((byte)'e', result[8]);
            Assert.Equal((byte)0, result[9]);
        }
        [Fact]
        public void SerializesMultipleProperties()
        {
            var result = Serializer.Serialize(new { Name = 4, Key = 13 });
            Assert.Equal((byte)'N', result[5]);
            Assert.Equal((byte)'a', result[6]);
            Assert.Equal((byte)'m', result[7]);
            Assert.Equal((byte)'e', result[8]);
            Assert.Equal((byte)0, result[9]);
            Assert.Equal((byte)'K', result[15]);
            Assert.Equal((byte)'e', result[16]);
            Assert.Equal((byte)'y', result[17]);
            Assert.Equal((byte)0, result[18]);
        }        
        [Fact]
        public void PutsEndOfDocumentByte()
        {
            var result = Serializer.Serialize(new {Name = 4});
            Assert.Equal((byte)0, result[result.Length-1]);
        }        
        [Fact]
        public void SeralizesAnInteger()
        {
            var result = Serializer.Serialize(new { Name = 4 });
            Assert.Equal(15, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(16, result[4]); //type
            Assert.Equal(4, BitConverter.ToInt32(result, 10));          
        }
        [Fact]
        public void SeralizesALong()
        {
            var result = Serializer.Serialize(new { Name = long.MinValue });
            Assert.Equal(19, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(18, result[4]); //type
            Assert.Equal(long.MinValue, BitConverter.ToInt64(result, 10));
        }
        [Fact]
        public void SerializesADateTime()
        {
            var date = new DateTime(2004, 4, 9, 10, 43, 23, 55);
            var result = Serializer.Serialize(new { Name = date });
            Assert.Equal(19, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(9, result[4]); //type
            Assert.Equal((long)date.Subtract(Helper.Epoch).TotalMilliseconds, BitConverter.ToInt64(result, 10));
        }             
        [Fact]
        public void SeralizesAFloatAsDouble()
        {
            var result = Serializer.Serialize(new { Name = 6.5f });
            Assert.Equal(19, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(1, result[4]); //type
            Assert.Equal(6.5f, (float)BitConverter.ToDouble(result, 10));
        }
        [Fact]
        public void SeralizesADouble()
        {
            var result = Serializer.Serialize(new { Name = Double.MaxValue });
            Assert.Equal(19, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(1, result[4]); //type
            Assert.Equal(Double.MaxValue, BitConverter.ToDouble(result, 10));
        }
        [Fact]
        public void SerializesAString()
        {
            var result = Serializer.Serialize(new { Name ="abc123" });
            Assert.Equal(22, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(2, result[4]); //type
            Assert.Equal(7, BitConverter.ToInt32(result, 10));
            Assert.Equal((byte)'a', result[14]);
            Assert.Equal((byte)'b', result[15]);
            Assert.Equal((byte)'c', result[16]);
            Assert.Equal((byte)'1', result[17]);
            Assert.Equal((byte)'2', result[18]);
            Assert.Equal((byte)'3', result[19]);
            Assert.Equal((byte)0, result[20]);
        }
        [Fact]
        public void SerializesTrue()
        {
            var result = Serializer.Serialize(new { Name = true });
            Assert.Equal(12, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(8, result[4]); //type
            Assert.Equal(1, result[10]);            
        }
        [Fact]
        public void SerializesFalse()
        {
            var result = Serializer.Serialize(new { Name = false });
            Assert.Equal(12, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(8, result[4]); //type
            Assert.Equal(0, result[10]);
        }
        [Fact]
        public void SerializesAnArray()
        {
            var result = Serializer.Serialize(new { Name = new object[]{4, "a"} });
            Assert.Equal(32, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(4, result[4]); //type
            Assert.Equal(21, BitConverter.ToInt32(result, 10)); //sub document length
            Assert.Equal(16, result[14]); //1st element type
            Assert.Equal((byte)'0', result[15]); //1st element name
            Assert.Equal(0, result[16]); //1st element name eoo
            Assert.Equal(4, BitConverter.ToInt32(result, 17));
            Assert.Equal(2, result[21]); //2nd element type
            Assert.Equal((byte)'1', result[22]); //2nd element name
            Assert.Equal(0, result[23]); //2nd element name eoo
            Assert.Equal(2, BitConverter.ToInt32(result, 24));
            Assert.Equal((byte)'a', result[28]);
            Assert.Equal((byte)0, result[29]);
            Assert.Equal((byte)0, result[30]);      //sub document eoo
            Assert.Equal((byte)0, result[31]);      //main document eoo       
        }
        [Fact]
        public void SerializesAList()
        {
            var result = Serializer.Serialize(new { Name = new List<int>{3,2,1} });
            Assert.Equal(37, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(4, result[4]); //type
            Assert.Equal(26, BitConverter.ToInt32(result, 10)); //sub document length
            Assert.Equal(16, result[14]); //1st element type
            Assert.Equal((byte)'0', result[15]); //1st element name
            Assert.Equal(0, result[16]); //1st element name eoo
            Assert.Equal(3, BitConverter.ToInt32(result, 17));
            Assert.Equal(16, result[21]); //2nd element type
            Assert.Equal((byte)'1', result[22]); //2nd element name
            Assert.Equal(0, result[23]); //2nd element name eoo
            Assert.Equal(2, BitConverter.ToInt32(result, 24));
            Assert.Equal(16, result[28]); //3rd element type
            Assert.Equal((byte)'2', result[29]); //3rd element name
            Assert.Equal(0, result[30]); //3rd element name eoo
            Assert.Equal(1, BitConverter.ToInt32(result, 31));
            Assert.Equal((byte)0, result[35]);      //sub document eoo
            Assert.Equal((byte)0, result[36]);      //main document eoo                
        }
        [Fact]
        public void SeralizesByteArrayAsABinary()
        {
            var result = Serializer.Serialize(new { Name = new byte[]{10, 12}});
            Assert.Equal(22, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(5, result[4]); //type
            Assert.Equal(6, BitConverter.ToInt32(result, 10)); //length
            Assert.Equal(2, result[14]); //subtype
            Assert.Equal(2, BitConverter.ToInt32(result, 15)); //array elements
            Assert.Equal(10, result[19]);
            Assert.Equal(12, result[20]);   
        }
        [Fact]
        public void SeralizesAGuid()
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            var result = Serializer.Serialize(new { Name = guid });
            Assert.Equal(32, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(5, result[4]); //type
            Assert.Equal(bytes.Length, BitConverter.ToInt32(result, 10)); //length
            Assert.Equal(3, result[14]); //subtype
            for(var i = 0; i < bytes.Length; ++i)
            {
                Assert.Equal(bytes[i], result[15+i]);
            }                   
        }
        [Fact]
        public void SerializesARegularExpression()
        {
            var result = Serializer.Serialize(new { Name = new Regex("9000", RegexOptions.Multiline | RegexOptions.IgnoreCase) });
            Assert.Equal(20, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(11, result[4]); //type
            Assert.Equal((byte)'9', result[10]);
            Assert.Equal((byte)'0', result[11]);
            Assert.Equal((byte)'0', result[12]);
            Assert.Equal((byte)'0', result[13]);
            Assert.Equal((byte)0, result[14]);
            Assert.Equal((byte)'i', result[15]);
            Assert.Equal((byte)'m', result[16]);
            Assert.Equal((byte)'u', result[17]); //always unicode
            Assert.Equal((byte)0, result[18]);
        }             
        [Fact]
        public void SerializesADictionary()
        {
            var result = Serializer.Serialize(new { Name = new Dictionary<string, object>{{"first", 1}, {"secOnd", "tWo"}}});
            Assert.Equal(43, BitConverter.ToInt32(result, 0)); //length
            Assert.Equal(3, result[4]); //type
            Assert.Equal(32, BitConverter.ToInt32(result, 10)); //subdocument length    
            Assert.Equal(16, result[14]); //1st argument type
            Assert.Equal((byte)'f', result[15]);
            Assert.Equal((byte)'i', result[16]);
            Assert.Equal((byte)'r', result[17]);
            Assert.Equal((byte)'s', result[18]);
            Assert.Equal((byte)'t', result[19]);
            Assert.Equal((byte)0, result[20]);
            Assert.Equal(1, BitConverter.ToInt32(result, 21));
            Assert.Equal(2, result[25]); //2nd argument type
            Assert.Equal((byte)'s', result[26]);
            Assert.Equal((byte)'e', result[27]);
            Assert.Equal((byte)'c', result[28]);
            Assert.Equal((byte)'O', result[29]);
            Assert.Equal((byte)'n', result[30]);
            Assert.Equal((byte)'d', result[31]);
            Assert.Equal((byte)0, result[32]);
            Assert.Equal(4, BitConverter.ToInt32(result, 33));
            Assert.Equal((byte)'t', result[37]);
            Assert.Equal((byte)'W', result[38]);
            Assert.Equal((byte)'o', result[39]);
            Assert.Equal((byte)0, result[40]);
            Assert.Equal((byte)0, result[41]); //sub doc eoo
            Assert.Equal((byte)0, result[42]);            
        }
        [Fact]
        public void ThrowsExceptionIfDictionaryKeyIsNotAString()
        {
            Assert.Throws<InvalidCastException>(() => Serializer.Serialize(new { Name = new Dictionary<int, object> { { 1, 1 } } }));
        }
        [Fact]
        public void SerializesNulls()
        {
            var result = Serializer.Serialize(new Skinny{ Nint = null });
            Assert.Equal(19, BitConverter.ToInt32(result, 0)); //length 
            Assert.Equal(10, result[4]); //type          
        }
        
        
    }
}