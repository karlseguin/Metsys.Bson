using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Metsys.Bson.Tests
{
	public class GenericTests
	{
		[Test]
		public void HandleGenericDictionary()
		{
			var dico = new Dictionary<string, object> {
				{ "string", "hello" },
				{ "integer", 123 },
				{ "long", 123L },
				{ "float", 123.45f },
				{ "double", 123.45d },
				{ "array", new List<object>{ 1, 2, 3 } },
				{ "object", new Dictionary<string, object> { { "hello", "world" } } },
			};

			var serialized = Serializer.Serialize( dico );

			var deserialized = Deserializer.Deserialize<Dictionary<string, object>>( serialized );

			Assert.IsNotNull( deserialized );
			Assert.IsInstanceOf<Dictionary<string, object>>( deserialized );
			Assert.AreEqual( dico.Count, deserialized.Count );

			foreach ( var item in dico )
			{
				Assert.IsTrue( deserialized.ContainsKey( item.Key ) );
				Assert.AreEqual( item.Value, deserialized[item.Key] );
			}
		}
	}
}

