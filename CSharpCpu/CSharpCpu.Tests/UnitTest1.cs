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
using CSharpCpu.Cpus;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestSwitch1()
		{
			TestTable(new uint[] { 0x00 }, new[] {
				new InstructionInfo(Name: "test1", Mask: new uint[] { 0xFF000000 }, Data: new uint[] { 0x12000000 }),
				new InstructionInfo(Name: "test2", Mask: new uint[] { 0xFF000000 }, Data: new uint[] { 0x20000000 }),
				new InstructionInfo(Name: "test3", Mask: new uint[] { 0xFF00000F }, Data: new uint[] { 0x33000000 }),
				new InstructionInfo(Name: "test4", Mask: new uint[] { 0xFF00000F }, Data: new uint[] { 0x33000001 }),
				new InstructionInfo(Name: "test5", Mask: new uint[] { 0xFF0000FF, 0xFF000000 }, Data: new uint[] { 0x33000042, 0x01000000 }),
			});
		}

		[TestMethod]
		public void TestSwitch2()
		{
			TestTable(new uint[] { 0x01 }, new[] {
				new InstructionInfo(Name: "test1", Mask: new uint[] { 0xFF, 0x00 }, Data: new uint[] { 0x00, 0x00 }),
			});
		}

		[TestMethod]
		public void TestSwitch3()
		{
			TestTable(new uint[] { 0x01 }, new[] {
				new InstructionInfo(Name: "test1", Mask: new uint[] { 0xFF, 0x00 }, Data: new uint[] { 0x00, 0x00 }),
				new InstructionInfo(Name: "test1", Mask: new uint[] { 0xFF, 0x00 }, Data: new uint[] { 0x01, 0x00 }),
			});
		}

		[TestMethod]
		public void TestSwitch4()
		{
			TestTable(new uint[] { 0xFF }, new[] {
				new InstructionInfo(Name: "test1", Mask: new uint[] { 0xFF, 0x00 }, Data: new uint[] { 0x00, 0x00 }),
				new InstructionInfo(Name: "test2", Mask: new uint[] { 0xFF }, Data: new uint[] { 0x01 }),
				new InstructionInfo(Name: "test3", Mask: new uint[] { 0xFF, 0x00, 0x0F }, Data: new uint[] { 0x01, 0x00, 0x01 }),
				new InstructionInfo(Name: "test4", Mask: new uint[] { 0xFF, 0x00, 0x0F }, Data: new uint[] { 0x01, 0x00, 0x02 }),
			});
		}

		private void TestTable(uint[] DefaultSequence, InstructionInfo[] Table)
		{
			const string DefaultValue = "!!DEFAULT!!";
			var SwitchTree = SwitchGenerator.GenerateSwitch(Table, (DecoderReference) =>
			{
				if (DecoderReference == null) return new AstNodeExprImm(DefaultValue);
				return new AstNodeExprImm(DecoderReference.Name);
			});
			
			SwitchTree = (AstNodeStm)(new AstOptimizer().Optimize(SwitchTree));

			var SwitchString = GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree);
			Console.WriteLine(SwitchString);

			var Func = GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);

			Func<uint[], string> Decode = (Data) =>
			{
				var Reader = new Queue<uint>(Data);
				return Func(() => {
					if (Reader.Count == 0) return 0x00;
					return Reader.Dequeue();
				});
			};

			foreach (var Item in Table)
			{
				var Decoded = Decode(Item.Data);
				Console.WriteLine(Decoded);
				Assert.AreEqual(Item.Name, Decoded);
			}
			Assert.AreEqual(DefaultValue, Decode(DefaultSequence));
		}
	}
}
