using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;
using System.Collections.Generic;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using SafeILGenerator.Ast;
using System.Text.RegularExpressions;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest2
	{
		static Regex MatchArgument = new Regex(@"%\w+", RegexOptions.Compiled);

		static public string Disassemble(string Key, uint[] _Values)
		{
			var Values = new Queue<uint>(_Values);

			return MatchArgument.Replace(Key, (Match) =>
			{
				return String.Format("${0:X}", Values.Dequeue());
			});
		}

		[TestMethod]
		public void TestMethod1()
		{
			var Ins = CSharpCpu.Cpus.Z80.InstructionTable.Instructions;

			var SwitchTree = SwitchGenerator.GenerateSwitch(Ins, (Context) =>
			{
				if (Context.DecoderReference == null) return "Unknown";

				var Array = new AstNodeExprNewArray(typeof(uint));

				MatchArgument.Replace(Context.DecoderReference.Name, (Match) => {
					var MatchStr = Match.ToString();
					switch (MatchStr)
					{
						case "%nn":
							Array.AddValue(
								(new AstNodeExprLocal(Context.Scope.Get("%n2")) * 256) |
								new AstNodeExprLocal(Context.Scope.Get("%n1"))
							);
							break;
						default:
							Array.AddValue(new AstNodeExprLocal(Context.Scope.Get(MatchStr)));
							break;
					}
					return "";
				});

				return new AstNodeExprCallStatic(
					((Func<string, uint[], string>)UnitTest2.Disassemble).Method,
					Context.DecoderReference.Name,
					Array
				);
			});


			//Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));

			var Decoder = GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);

			var Items = new Queue<uint>(new uint[] { 0xAF, 0x21, 0xFF, 0xDF, 0x0E, 0x10, 0x06, 0x00, 0x32, 0x05, 0x20, 0xFC, 0x0D, 0x20, 0xF9, 0x3E });
			Func<uint> Reader = () => {
				var Result = Items.Dequeue();
				Console.Write("{0:X2} ", Result);
				return Result;
			};

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


			for (int n = 0; n < 7; n++)
			{
				Console.WriteLine("\n  : {0}", Decoder(Reader));
			}

			Assert.Fail();
		}
	}
}
