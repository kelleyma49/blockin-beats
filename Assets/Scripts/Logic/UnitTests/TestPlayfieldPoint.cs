#if ENABLE_UNITTESTS
using NUnit.Framework;
using System;

namespace Logic.Tests
{
	[TestFixture()]
	public class TestPlayfieldPoint
	{
		[Test()]
		public void TestProperties()
		{
			var p = new Logic.PlayfieldPoint(1, 2);
			Assert.AreEqual(1, p.Column);
			Assert.AreEqual(2, p.Row);
		}

		[Test()]
		public void TestToString()
		{
			var p = new Logic.PlayfieldPoint(2, 3);
			Assert.AreEqual("2,3", p.ToString());
		}
	}
}
#endif // ENABLE_UNITTESTS
