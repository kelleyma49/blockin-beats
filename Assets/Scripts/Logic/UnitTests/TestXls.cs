 
#if ENABLE_UNITTESTS
using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Logic;

namespace UnitTests
{
	[TestFixture()]
	public class TestXls
	{
		[Test()]
		public void TestCompleteBlockDisabled()
		{
			const string path = @"..\..\..\TestCases\TestCompleteBlockDisabled.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestCompleteBlockTall()
		{
			const string path = @"..\..\..\TestCases\TestCompleteBlockTall.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestDroppedBlock()
		{
			const string path = @"..\..\..\TestCases\TestDroppedBlock.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestNotActivated()
		{
			const string path = @"..\..\..\TestCases\TestNotActivated.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimple2x2()
		{
			const string path = @"..\..\..\TestCases\TestSimple2x2.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimple3x2BlocksBonus()
		{
			const string path = @"..\..\..\TestCases\TestSimple3x2BlocksBonus.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimple3x2DoubleBlocks()
		{
			const string path = @"..\..\..\TestCases\TestSimple3x2DoubleBlocks.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimple3x2SingleBlock()
		{
			const string path = @"..\..\..\TestCases\TestSimple3x2SingleBlock.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleCompleteBlock()
		{
			const string path = @"..\..\..\TestCases\TestSimpleCompleteBlock.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleCompleteDoubleBlockAndDrop()
		{
			const string path = @"..\..\..\TestCases\TestSimpleCompleteDoubleBlockAndDrop.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweled()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweled.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweledHorz()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweledHorz.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweledMultiple()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweledMultiple.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweledNotInSquare()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweledNotInSquare.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweledPastEnd()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweledPastEnd.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleJeweledVert()
		{
			const string path = @"..\..\..\TestCases\TestSimpleJeweledVert.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestSimpleWithBlackAndWhite2x2()
		{
			const string path = @"..\..\..\TestCases\TestSimpleWithBlackAndWhite2x2.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}

		[Test()]
		public void TestStackedSquares()
		{
			const string path = @"..\..\..\TestCases\TestStackedSquares.xlsx";
			var fullPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory,path);
			ExcelLoader.LoadAndRun(fullPath);
		}
	}
}
#endif // ENABLE_UNITTESTS
