using Xunit;

namespace Metsys.Bson.Tests
{
    public class FailTests
    {
        [Fact]
        //problem seems related to Child.Name not actually overriding Parent.Name as far as GetProperties is concerned
        public void ItWouldBeGreatIfThisTestDidNotFail()
        {
            
            Serializer.Serialize(new Child());
        }
    }

    public class Parent<T>
    {
        public virtual T Name { get; set; }
    }

    public class Child : Parent<string>
    {
        public override string Name { get; set; }
    }
}