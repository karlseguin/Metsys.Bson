using System;

namespace Metsys.Bson
{
    public class ObjectId
    {
        private string _string;

        public ObjectId()
        {
        }

        public ObjectId(string value) : this(DecodeHex(value))
        {
        }

        internal ObjectId(byte[] value)
        {
            Value = value;
        }

        public static ObjectId Empty
        {
            get { return new ObjectId("000000000000000000000000"); }
        }

        public byte[] Value { get; private set; }

        public static ObjectId NewObjectId()
        {
            // TODO: generate random-ish bits.
            return new ObjectId { Value = ObjectIdGenerator.Generate() };
        }

        public static bool TryParse(string value, out ObjectId id)
        {
            id = Empty;
            if (value == null || value.Length != 24)
            {
                return false;
            }

            try
            {
                id = new ObjectId(value);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool operator ==(ObjectId a, ObjectId b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ObjectId a, ObjectId b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return Value != null ? ToString().GetHashCode() : 0;
        }

        public override string ToString()
        {
            if (_string == null && Value != null)
            {
                _string = BitConverter.ToString(Value).Replace("-", string.Empty).ToLower();
            }

            return _string;
        }

        public override bool Equals(object o)
        {
            var other = o as ObjectId;
            return Equals(other);
        }

        public bool Equals(ObjectId other)
        {
            return other != null && ToString() == other.ToString();
        }

        protected static byte[] DecodeHex(string val)
        {
            var chars = val.ToCharArray();
            var numberChars = chars.Length;
            var bytes = new byte[numberChars / 2];

            for (var i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(new string(chars, i, 2), 16);
            }

            return bytes;
        }
        
        public static implicit operator string(ObjectId oid)
        {
            return oid.ToString();
        }
        public static implicit operator ObjectId(string oidString)
        {
            return new ObjectId(oidString);
        }
    }
}