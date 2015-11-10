using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Metsys.Bson.Tests
{
    public class SerializationTests
    {
        [Test]
        public void SerializesAProperty()
        {
            var result = Serializer.Serialize(new {Name = 4});
            Assert.AreEqual((byte)'N', result[5]);
            Assert.AreEqual((byte)'a', result[6]);
            Assert.AreEqual((byte)'m', result[7]);
            Assert.AreEqual((byte)'e', result[8]);
            Assert.AreEqual((byte)0, result[9]);
        }
        [Test]
        public void SerializesMultipleProperties()
        {
            var result = Serializer.Serialize(new { Name = 4, Key = 13 });
            Assert.AreEqual((byte)'N', result[5]);
            Assert.AreEqual((byte)'a', result[6]);
            Assert.AreEqual((byte)'m', result[7]);
            Assert.AreEqual((byte)'e', result[8]);
            Assert.AreEqual((byte)0, result[9]);
            Assert.AreEqual((byte)'K', result[15]);
            Assert.AreEqual((byte)'e', result[16]);
            Assert.AreEqual((byte)'y', result[17]);
            Assert.AreEqual((byte)0, result[18]);
        }        
        [Test]
        public void PutsEndOfDocumentByte()
        {
            var result = Serializer.Serialize(new {Name = 4});
            Assert.AreEqual((byte)0, result[result.Length-1]);
        }        
        [Test]
        public void SeralizesAnInteger()
        {
            var result = Serializer.Serialize(new { Name = 4 });
            Assert.AreEqual(15, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(16, result[4]); //type
            Assert.AreEqual(4, BitConverter.ToInt32(result, 10));          
        }
        [Test]
        public void SeralizesALong()
        {
            var result = Serializer.Serialize(new { Name = long.MinValue });
            Assert.AreEqual(19, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(18, result[4]); //type
            Assert.AreEqual(long.MinValue, BitConverter.ToInt64(result, 10));
        }
        [Test]
        public void SerializesADateTime()
        {
            var date = new DateTime(2004, 4, 9, 10, 43, 23, 55);
            var result = Serializer.Serialize(new { Name = date });
            Assert.AreEqual(19, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(9, result[4]); //type
            Assert.AreEqual((long)date.Subtract(Helper.Epoch).TotalMilliseconds, BitConverter.ToInt64(result, 10));
        }             
        [Test]
        public void SeralizesAFloatAsDouble()
        {
            var result = Serializer.Serialize(new { Name = 6.5f });
            Assert.AreEqual(19, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(1, result[4]); //type
            Assert.AreEqual(6.5f, (float)BitConverter.ToDouble(result, 10));
        }
        [Test]
        public void SeralizesADouble()
        {
            var result = Serializer.Serialize(new { Name = Double.MaxValue });
            Assert.AreEqual(19, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(1, result[4]); //type
            Assert.AreEqual(Double.MaxValue, BitConverter.ToDouble(result, 10));
        }
        [Test]
        public void SerializesAString()
        {
            var result = Serializer.Serialize(new { Name ="abc123" });
            Assert.AreEqual(22, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(2, result[4]); //type
            Assert.AreEqual(7, BitConverter.ToInt32(result, 10));
            Assert.AreEqual((byte)'a', result[14]);
            Assert.AreEqual((byte)'b', result[15]);
            Assert.AreEqual((byte)'c', result[16]);
            Assert.AreEqual((byte)'1', result[17]);
            Assert.AreEqual((byte)'2', result[18]);
            Assert.AreEqual((byte)'3', result[19]);
            Assert.AreEqual((byte)0, result[20]);
        }
        [Test]
        public void SerializesTrue()
        {
            var result = Serializer.Serialize(new { Name = true });
            Assert.AreEqual(12, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(8, result[4]); //type
            Assert.AreEqual(1, result[10]);            
        }
        [Test]
        public void SerializesFalse()
        {
            var result = Serializer.Serialize(new { Name = false });
            Assert.AreEqual(12, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(8, result[4]); //type
            Assert.AreEqual(0, result[10]);
        }
        [Test]
        public void SerializesAnArray()
        {
            var result = Serializer.Serialize(new { Name = new object[]{4, "a"} });
            Assert.AreEqual(32, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(4, result[4]); //type
            Assert.AreEqual(21, BitConverter.ToInt32(result, 10)); //sub document length
            Assert.AreEqual(16, result[14]); //1st element type
            Assert.AreEqual((byte)'0', result[15]); //1st element name
            Assert.AreEqual(0, result[16]); //1st element name eoo
            Assert.AreEqual(4, BitConverter.ToInt32(result, 17));
            Assert.AreEqual(2, result[21]); //2nd element type
            Assert.AreEqual((byte)'1', result[22]); //2nd element name
            Assert.AreEqual(0, result[23]); //2nd element name eoo
            Assert.AreEqual(2, BitConverter.ToInt32(result, 24));
            Assert.AreEqual((byte)'a', result[28]);
            Assert.AreEqual((byte)0, result[29]);
            Assert.AreEqual((byte)0, result[30]);      //sub document eoo
            Assert.AreEqual((byte)0, result[31]);      //main document eoo       
        }
        [Test]
        public void SerializesAList()
        {
            var result = Serializer.Serialize(new { Name = new List<int>{3,2,1} });
            Assert.AreEqual(37, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(4, result[4]); //type
            Assert.AreEqual(26, BitConverter.ToInt32(result, 10)); //sub document length
            Assert.AreEqual(16, result[14]); //1st element type
            Assert.AreEqual((byte)'0', result[15]); //1st element name
            Assert.AreEqual(0, result[16]); //1st element name eoo
            Assert.AreEqual(3, BitConverter.ToInt32(result, 17));
            Assert.AreEqual(16, result[21]); //2nd element type
            Assert.AreEqual((byte)'1', result[22]); //2nd element name
            Assert.AreEqual(0, result[23]); //2nd element name eoo
            Assert.AreEqual(2, BitConverter.ToInt32(result, 24));
            Assert.AreEqual(16, result[28]); //3rd element type
            Assert.AreEqual((byte)'2', result[29]); //3rd element name
            Assert.AreEqual(0, result[30]); //3rd element name eoo
            Assert.AreEqual(1, BitConverter.ToInt32(result, 31));
            Assert.AreEqual((byte)0, result[35]);      //sub document eoo
            Assert.AreEqual((byte)0, result[36]);      //main document eoo                
        }
        [Test]
        public void SerializesAHashSet()
        {
            var result = Serializer.Serialize(new { Name = new HashSet<int> { 3, 2, 1 } });
            Assert.AreEqual(37, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(4, result[4]); //type
            Assert.AreEqual(26, BitConverter.ToInt32(result, 10)); //sub document length
            Assert.AreEqual(16, result[14]); //1st element type
            Assert.AreEqual((byte)'0', result[15]); //1st element name
            Assert.AreEqual(0, result[16]); //1st element name eoo
            Assert.AreEqual(3, BitConverter.ToInt32(result, 17));
            Assert.AreEqual(16, result[21]); //2nd element type
            Assert.AreEqual((byte)'1', result[22]); //2nd element name
            Assert.AreEqual(0, result[23]); //2nd element name eoo
            Assert.AreEqual(2, BitConverter.ToInt32(result, 24));
            Assert.AreEqual(16, result[28]); //3rd element type
            Assert.AreEqual((byte)'2', result[29]); //3rd element name
            Assert.AreEqual(0, result[30]); //3rd element name eoo
            Assert.AreEqual(1, BitConverter.ToInt32(result, 31));
            Assert.AreEqual((byte)0, result[35]);      //sub document eoo
            Assert.AreEqual((byte)0, result[36]);      //main document eoo                
        }        
        [Test]
        public void SeralizesByteArrayAsABinary()
        {
            var result = Serializer.Serialize(new { Name = new byte[]{10, 12}});
            Assert.AreEqual(22, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(5, result[4]); //type
            Assert.AreEqual(6, BitConverter.ToInt32(result, 10)); //length
            Assert.AreEqual(2, result[14]); //subtype
            Assert.AreEqual(2, BitConverter.ToInt32(result, 15)); //array elements
            Assert.AreEqual(10, result[19]);
            Assert.AreEqual(12, result[20]);   
        }
        [Test]
        public void SeralizesAGuid()
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();
            var result = Serializer.Serialize(new { Name = guid });
            Assert.AreEqual(32, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(5, result[4]); //type
            Assert.AreEqual(bytes.Length, BitConverter.ToInt32(result, 10)); //length
            Assert.AreEqual(3, result[14]); //subtype
            for(var i = 0; i < bytes.Length; ++i)
            {
                Assert.AreEqual(bytes[i], result[15+i]);
            }                   
        }
        [Test]
        public void SerializesARegularExpression()
        {
            var result = Serializer.Serialize(new { Name = new Regex("9000", RegexOptions.Multiline | RegexOptions.IgnoreCase) });
            Assert.AreEqual(20, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(11, result[4]); //type
            Assert.AreEqual((byte)'9', result[10]);
            Assert.AreEqual((byte)'0', result[11]);
            Assert.AreEqual((byte)'0', result[12]);
            Assert.AreEqual((byte)'0', result[13]);
            Assert.AreEqual((byte)0, result[14]);
            Assert.AreEqual((byte)'i', result[15]);
            Assert.AreEqual((byte)'m', result[16]);
            Assert.AreEqual((byte)'u', result[17]); //always unicode
            Assert.AreEqual((byte)0, result[18]);
        }             
        [Test]
        public void SerializesADictionary()
        {
            var result = Serializer.Serialize(new { Name = new Dictionary<string, object>{{"first", 1}, {"secOnd", "tWo"}}});
            Assert.AreEqual(43, BitConverter.ToInt32(result, 0)); //length
            Assert.AreEqual(3, result[4]); //type
            Assert.AreEqual(32, BitConverter.ToInt32(result, 10)); //subdocument length    
            Assert.AreEqual(16, result[14]); //1st argument type
            Assert.AreEqual((byte)'f', result[15]);
            Assert.AreEqual((byte)'i', result[16]);
            Assert.AreEqual((byte)'r', result[17]);
            Assert.AreEqual((byte)'s', result[18]);
            Assert.AreEqual((byte)'t', result[19]);
            Assert.AreEqual((byte)0, result[20]);
            Assert.AreEqual(1, BitConverter.ToInt32(result, 21));
            Assert.AreEqual(2, result[25]); //2nd argument type
            Assert.AreEqual((byte)'s', result[26]);
            Assert.AreEqual((byte)'e', result[27]);
            Assert.AreEqual((byte)'c', result[28]);
            Assert.AreEqual((byte)'O', result[29]);
            Assert.AreEqual((byte)'n', result[30]);
            Assert.AreEqual((byte)'d', result[31]);
            Assert.AreEqual((byte)0, result[32]);
            Assert.AreEqual(4, BitConverter.ToInt32(result, 33));
            Assert.AreEqual((byte)'t', result[37]);
            Assert.AreEqual((byte)'W', result[38]);
            Assert.AreEqual((byte)'o', result[39]);
            Assert.AreEqual((byte)0, result[40]);
            Assert.AreEqual((byte)0, result[41]); //sub doc eoo
            Assert.AreEqual((byte)0, result[42]);            
        }
        [Test]
        public void ThrowsExceptionIfDictionaryKeyIsNotAString()
        {
            Assert.Throws<InvalidCastException>(() => Serializer.Serialize(new { Name = new Dictionary<int, object> { { 1, 1 } } }));
        }
        [Test]
        public void SerializesNulls()
        {
            var result = Serializer.Serialize(new Skinny{ Nint = null });
            Assert.AreEqual(19, BitConverter.ToInt32(result, 0)); //length 
            Assert.AreEqual(10, result[4]); //type          
        }
       
        [Test]
        public void ThrowsExceptionOnInvalidRootTypes()
        {
           var message = "Root type must be an non-enumerable object";
           Assert.AreEqual(message, Assert.Throws<BsonException>(() => Serializer.Serialize(3)).Message);
           Assert.AreEqual(message, Assert.Throws<BsonException>(() => Serializer.Serialize(new[]{1,2,3})).Message);
        }
    }
}