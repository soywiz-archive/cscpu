using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest2
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Ins = CSharpCpu.Cpus.Z80.InstructionTable.Instructions;
			SwitchGenerator.GenerateSwitch(Ins, (Item) => {
				if (Item == null) return "Unknown";
				return Item.Name;
			});
		}
	}
}
