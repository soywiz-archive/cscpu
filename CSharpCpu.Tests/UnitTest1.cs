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
				new InstructionInfo("test1", "", new MaskDataVars(0xFF000000, 0x12000000)),
				new InstructionInfo("test2", "", new MaskDataVars(0xFF000000, 0x20000000)),
				new InstructionInfo("test3", "", new MaskDataVars(0xFF00000F, 0x33000000)),
				new InstructionInfo("test4", "", new MaskDataVars(0xFF00000F, 0x33000001)),
				new InstructionInfo("test5", "", new MaskDataVars(0xFF0000FF, 0x33000042), new MaskDataVars(0xFF000000, 0x01000000)),
			});
		}

		[TestMethod]
		public void TestSwitch2a()
		{
			TestTable(new uint[] { 0x01 }, new[] {
				new InstructionInfo("test1", "", new MaskDataVars(0xFF, 0x00), new MaskDataVars(0x00, 0x00)),
			});
		}

		[TestMethod]
		public void TestSwitch2b()
		{
			TestTable(new uint[] { 0x01 }, new[] {
				new InstructionInfo("test1", "", new MaskDataVars(0xFF, 0x00), new MaskDataVars(0x00, 0x00), new MaskDataVars(0x00, 0x00)),
			});
		}

		[TestMethod]
		public void TestSwitch2c()
		{
			TestTable(new uint[] { 0x01 }, new[] {
				new InstructionInfo("test1", "", new MaskDataVars(0xFF, 0x00), new MaskDataVars(0x00, 0x00), new MaskDataVars(0x00, 0x00), new MaskDataVars(0x00, 0x00)),
			});
		}

		[TestMethod]
		public void TestSwitch3()
		{
			TestTable(new uint[] { 0xFF }, new[] {
				new InstructionInfo("test1", "", new MaskDataVars(0xFF, 0x00), new MaskDataVars(0x00, 0x00)),
				new InstructionInfo("test2", "", new MaskDataVars(0xFF, 0x01), new MaskDataVars(0x00, 0x00)),
			});
		}

		[TestMethod]
		public void TestSwitch4()
		{
			TestTable(new uint[] { 0xFF }, new[] {
				new InstructionInfo("test1", "", new MaskDataVars(0xFF, 0x00), new MaskDataVars(0x00, 0x00)),
				new InstructionInfo("test2", "", new MaskDataVars(0xFF, 0x02)),
				new InstructionInfo("test3", "", new MaskDataVars(0xFF, 0x01), new MaskDataVars(0x00, 0x00), new MaskDataVars(0x0F, 0x01)),
				new InstructionInfo("test4", "", new MaskDataVars(0xFF, 0x01), new MaskDataVars(0x00, 0x00), new MaskDataVars(0x0F, 0x02)),
			});
		}

		private void TestTable(uint[] DefaultSequence, InstructionInfo[] Table)
		{
			const string DefaultValue = "!!DEFAULT!!";
			var SwitchTree = SwitchGenerator.GenerateSwitch(Table, (Context) =>
			{
				if (Context.DecoderReference == null) return new AstNodeExprImm(DefaultValue);
				return new AstNodeExprImm(Context.DecoderReference.Name);
			});
			
			SwitchTree = (AstNodeStm)(new AstOptimizer().Optimize(SwitchTree));

			var SwitchString = GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree);
			Console.WriteLine(SwitchString);

			var Func = GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);

			Func<uint[], string> Decode = (Data) =>
			{
				var Reader = new Queue<uint>(Data);
				var Result = Func(() => {
					if (Reader.Count == 0) return 0x00;
					return Reader.Dequeue();
				});
				Assert.AreEqual(0, Reader.Count);
				return Result;
			};

			foreach (var Item in Table)
			{
				var Decoded = Decode(Item.MaskDataVarsList.Select(MaskDataVars => MaskDataVars.Data).ToArray());
				Console.WriteLine(Decoded);
				Assert.AreEqual(Item.Name, Decoded);
			}
			Assert.AreEqual(DefaultValue, Decode(DefaultSequence));
		}
	}
}
