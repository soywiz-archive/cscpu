using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;
using System.Collections.Generic;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest2
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Ins = CSharpCpu.Cpus.Z80.InstructionTable.Instructions;
			var SwitchTree = SwitchGenerator.GenerateSwitch(Ins, (Context) =>
			{
				if (Context.DecoderReference == null) return "Unknown";
				return Context.DecoderReference.Name + " [" + String.Join(",", Context.Scope.GetAllKeys()) + "] ";
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));

			var Decoder = GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);

			var Items = new Queue<uint>(new uint[] { 0xAF, 0x21, 0xFF, 0xDF, 0x0E, 0x10, 0x06, 0x00, 0x32, 0x05, 0x20, 0xFC, 0x0D, 0x20, 0xF9, 0x3E });
			Func<uint> Reader = () => Items.Dequeue();

			//ROM:020C sub_20C:                                ; CODE XREF: sub_0j
			//ROM:020C                 xor     a
			//ROM:020D                 ld      hl, $DFFF
			//ROM:0210                 ld      c, $10
			//ROM:0212                 ld      b, 0
			//ROM:0214
			//ROM:0214 loc_214:                                ; CODE XREF: sub_20C+Aj
			//ROM:0214                                         ; sub_20C+Dj
			//ROM:0214                 ldd     [hl], a
			//ROM:0215                 dec     b
			//ROM:0216                 jr      nz, loc_214
			//ROM:0218                 dec     c
			//ROM:0219                 jr      nz, loc_214
			//ROM:021B


			Console.WriteLine(Decoder(Reader));
			Console.WriteLine(Decoder(Reader));
			Console.WriteLine(Decoder(Reader));
			Console.WriteLine(Decoder(Reader));
			Console.WriteLine(Decoder(Reader));
			Console.WriteLine(Decoder(Reader));
			//Console.WriteLine(Decoder(Reader));

			Assert.Fail();
		}
	}
}
