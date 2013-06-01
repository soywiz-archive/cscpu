using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			var SwitchGenerator = new SwitchGenerator();
			var GeneratorCSharp = new GeneratorCSharp();
			Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchGenerator.GenerateSwitch(new[] {
				new DecoderReference() { Mask = new[] { (uint)0xFF000000 }, Data = new[] { (uint)0x12000000 } },
				new DecoderReference() { Mask = new[] { (uint)0xFF000000 }, Data = new[] { (uint)0x20000000 } },
			})));
		}

		class DecoderReference : IDecoderReference {
			public uint[] Data;
			public uint[] Mask;

			uint[] IDecoderReference.Data
			{
				get { return Data; }
			}

			uint[] IDecoderReference.Mask
			{
				get { return Mask; }
			}
		}
	}
}
