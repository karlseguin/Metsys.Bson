using NUnit.Framework;

namespace Metsys.Bson.Tests
{
    public class ObjectIdTests
    {
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsNull()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(null, out objectId));
        }
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsEmpty()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(string.Empty, out objectId));
        }
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsnt24Characters()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse("a", out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('b', 23), out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('b', 25), out objectId));
        }
        [Test]
        public void TryParseReturnsFalseIfObjectIdIsinvalid()
        {
            ObjectId objectId;
            Assert.AreEqual(false, ObjectId.TryParse(new string('*', 24), out objectId));
            Assert.AreEqual(false, ObjectId.TryParse(new string('1', 23) + '-', out objectId));
        }
        [Test]
        public void ReturnsParsedObjectId()
        {
            ObjectId objectId;
            Assert.AreEqual(true, ObjectId.TryParse("4b883faad657000000002665", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
            Assert.AreEqual(true, ObjectId.TryParse("1234567890abcdef123456ab", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
            Assert.AreEqual(true, ObjectId.TryParse("1234567890abCDEf123456ab", out objectId));
            Assert.AreNotEqual(ObjectId.Empty, objectId);
        }
        [Test]
        public void ObjectIdWithSameValueAreEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002665");
            Assert.AreEqual(a, b);
            Assert.True(a == b);
        }
        [Test]
        public void ObjectIdWithDifferentValuesAreNotEqual()
        {
            var a = new ObjectId("4b883faad657000000002665");
            var b = new ObjectId("4b883faad657000000002666");
            Assert.AreNotEqual(a, b);
            Assert.True(a != b);
        }
        [Test]
        public void ImplicitConversionOfObjectIdToAndFromStringWorks()
        {
            var oid = ObjectId.NewObjectId();
            string str = oid;
            Assert.AreEqual(oid, (ObjectId)str);
        }
        
        [Test]
        public void ConversionToStringWithNullObjIsNull()
        {
            var obj = new { Id = (ObjectId)null };            
            Assert.AreEqual(null, obj.Id);
        }
    }
}