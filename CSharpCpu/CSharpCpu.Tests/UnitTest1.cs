using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator;
using System.Reflection.Emit;
using SafeILGenerator.Ast.Optimizers;
using System.Collections.Generic;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest1
	{
		static DecoderReference[] Table = new[] {
			new DecoderReference() { Name = "test1", Mask = new[] { (uint)0xFF000000 }, Data = new[] { (uint)0x12000000 } },
			new DecoderReference() { Name = "test2", Mask = new[] { (uint)0xFF000000 }, Data = new[] { (uint)0x20000000 } },
			new DecoderReference() { Name = "test3", Mask = new[] { (uint)0xFF00000F }, Data = new[] { (uint)0x33000000 } },
			new DecoderReference() { Name = "test4", Mask = new[] { (uint)0xFF00000F }, Data = new[] { (uint)0x33000001 } },
			new DecoderReference() { Name = "test5", Mask = new[] { (uint)0xFF0000FF, (uint)0xFF000000 }, Data = new[] { (uint)0x33000042, (uint)0x01000000 } },
		};

		[TestMethod]
		public void TestSwitch()
		{
			var GeneratorCSharp = new GeneratorCSharp();

			const string DefaultValue = "DEFAULT!";
			
			var SwitchTree = SwitchGenerator.GenerateSwitch(Table, (DecoderReference) =>
			{
				if (DecoderReference == null) return new AstNodeExprImm(DefaultValue);
				return new AstNodeExprImm(DecoderReference.Name);
			});
			SwitchTree = (AstNodeStm)(new AstOptimizer().Optimize(SwitchTree));
			var StringString = GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree);
			var Func = GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);

			Func<uint[], string> Decode = (Data) => {
				var Reader = new Queue<uint>(Data);
				return Func(() => Reader.Dequeue());
			};

			for (int n = 0; n < 5; n++) {
				Assert.AreEqual(Table[n].Name, Decode(Table[n].Data));
			}
			Assert.AreEqual(DefaultValue, Decode(new uint[] { 0 }));
		}

		class DecoderReference : IDecoderReference {
			public string Name;
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
