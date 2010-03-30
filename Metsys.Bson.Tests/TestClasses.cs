using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Metsys.Bson.Tests
{
    public class Skinny
    {
        public int? Nint{ get; set;}
        public string String{ get; set;}
    }
    
    public class Fatty
    {
        private HashSet<int> _setterLesshashSet;
        private IList<int> _setterLessList;
               
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
        public IList<string> List{ get; private set;}        
        public IList<string> SetterList{ get; private set;}
        public HashSet<string> HashSet { get; private set;}        
        public IList<int> SetterLessList
        {
            get
            {
                if (_setterLessList == null)
                {
                    _setterLessList = new List<int>();
                }
                return _setterLessList;
            }
        }
        public HashSet<int> SetterlessHashSet
        {
            get
            {
                if (_setterLesshashSet == null)
                {
                    _setterLesshashSet = new HashSet<int>();
                }
                return _setterLesshashSet;
            }
        }
        public IDictionary<string, object> Dictionary{ get; set;}
        public Fatty Child{ get; set;}        
    }
    
    public class Private
    {
        public string Key{ get; set;}
        private Private(){}
    }
    
    public class Impossible
    {
        public string Key{ get; set;}
        public Impossible(string key)
        {
            Key = key;
        }
    }
    
    public class Expandotator : IExpando
    {
        private IDictionary<string, object> _expando;
        public string Key{ get; set;}
        
        public IDictionary<string, object> Expando
        {
            get
            {
                if (_expando == null) { _expando = new Dictionary<string, object>(); }
                return _expando;
            }
        }
    }
}