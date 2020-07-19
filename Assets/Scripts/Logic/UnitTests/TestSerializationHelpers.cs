#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;
using System.Runtime.Serialization;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestSerializationHelpers
	{
		[DataContract]
		class TestClass
		{
			public void Init()
			{
				TestInt = 123;
			}

			[DataMember]
			public int TestInt { get; set; }
		}

		[Test()]
		public void TestWriteObj()
		{
			var tc = new TestClass();
			Assert.AreNotSame(null, Logic.SerializeHelpers.WriteObject(tc));
		}

		[Test()]
		public void TestWriteAndReadObj()
		{
			var tc1 = new TestClass();
			tc1.Init();
			using (var memStream = Logic.SerializeHelpers.WriteObject(tc1))
			{
				var tc2 = Logic.SerializeHelpers.ReadObject<TestClass>(memStream);
				Assert.AreNotSame(tc1, tc2);
				Assert.AreEqual(tc1.TestInt, tc2.TestInt);
			}
		}

		[Test()]
		public void TestClone()
		{
			var tc1 = new TestClass();
			tc1.Init();
			var tc2 = Logic.SerializeHelpers.DeepClone(tc1);
			Assert.AreNotSame(tc1, tc2);
			Assert.AreEqual(tc1.TestInt, tc2.TestInt);
		}
	}
}
#endif // ENABLE_UNITTESTS
