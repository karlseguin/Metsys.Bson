using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Metsys.Bson.Tests
{
    public class Skinny
    {
        public int? Nint{ get; set;}
    }
    public class Fatty
    {
        public int Int{ get; set;}
        public int? Nint{ get; set;}
        public float Float{ get; set;}
        public float? Nfloat{ get; set;}
        public long Long{ get; set;}
        public long? Nlong{ get; set;}
        public double Double{ get; set;}
        public double? NDouble{ get; set;}
        public string String{ get; set;}
        public Guid Guid { get; set; }
        public Guid? Nguid { get; set; }
        public Regex Regex{ get; set;}
        public DateTime DateTime{ get; set;}
        public DateTime? NdateTime{ get; set;}
        public byte[] ByteArray{ get; set;}
        public object[] Array{ get; set;}
        public IList<string> List{ get; set;}
        public IDictionary<string, object> Dictionary{ get; set;}
    }
}