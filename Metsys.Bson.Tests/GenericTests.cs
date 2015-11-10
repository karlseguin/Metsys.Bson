using NUnit.Framework;
using Metsys.Bson.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

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
				{ "null", null },
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

		[Test]
		public void UseLongIntegers()
		{
			var now = new DateTime( 2000, 1, 1, 12, 34, 56, DateTimeKind.Utc );

			var dico = new Dictionary<string, object> {
				{ "int32", (int) 123 },
				{ "long", (long) 123L },
				{ "float", 123.45f },
				{ "double", 123.45d },
				{ "date", now },
			};

			var serialized = Serializer.Serialize( dico );
			var deserialized = Deserializer.Deserialize<Dictionary<string, object>>( serialized );

			Assert.IsTrue( deserialized["int32"] is int );
			Assert.IsTrue( deserialized["long"] is long );
			Assert.IsTrue( deserialized["float"] is double );
			Assert.IsTrue( deserialized["double"] is double );
			Assert.IsTrue( deserialized["date"] is DateTime );
			Assert.AreEqual( now, deserialized["date"] );

			deserialized = Deserializer.Deserialize<Dictionary<string, object>>( serialized, new Deserializer.Options{ LongIntegers = true, StringDates = true } );

			Assert.IsTrue( deserialized["int32"] is long );
			Assert.IsTrue( deserialized["long"] is long );
			Assert.IsTrue( deserialized["float"] is double );
			Assert.IsTrue( deserialized["double"] is double );
			Assert.IsTrue( deserialized["date"] is string );
			Assert.AreEqual( now, DateTime.Parse( (string) deserialized["date"] ) );
		}

		[TearDown]
		public void Dispose()
		{
			typeof(BsonConfiguration).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
		}
	}
}

