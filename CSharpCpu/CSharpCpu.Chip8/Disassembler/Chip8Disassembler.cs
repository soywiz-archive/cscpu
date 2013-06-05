using CSharpCpu.Decoder;
using CSharpCpu.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8.Disassembler
{
	public class Chip8Disassembler
	{
		static private AstGenerator ast = AstGenerator.Instance;

		private IMemory2 Memory;
		public ushort Address;
		private Func<SwitchReadWordDelegate, string> Decoder;

		public Chip8Disassembler(IMemory2 Memory)
		{
			this.Memory = Memory;
			this.Decoder = LazyDecoder.Value;
		}

		public string DecodeNext()
		{
			return this.Decoder(() => Memory.Read2(this.Address += 2));
		}

		private static Regex MatchArgument = new Regex(@"%\w+", RegexOptions.Compiled);

		static public string __DisassembleCallback(string Key, uint[] _Values)
		{
			var Values = new Queue<uint>(_Values);

			return MatchArgument.Replace(Key, (Match) =>
			{
				return String.Format("${0:X}", Values.Dequeue());
			});
		}

		static private Lazy<Func<SwitchReadWordDelegate, string>> LazyDecoder = new Lazy<Func<SwitchReadWordDelegate, string>>(() =>
		{
			var Ins = InstructionTable.Instructions;

			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<string, InstructionInfo>(Ins, (Context) =>
			{
				if (Context.DecoderReference == null) return ast.Return("Unknown");

				var Array = new AstNodeExprNewArray(typeof(uint));

				MatchArgument.Replace(Context.DecoderReference.Name, (Match) =>
				{
					var MatchStr = Match.ToString();
					switch (MatchStr)
					{
						case "%addr": Array.AddValue(new AstNodeExprLocal(Context.Scope.Get("nnn"))); break;
						default: throw (new Exception("MatchStr"));
					}
					return "";
				});

				return ast.Return(ast.CallStatic(
					((Func<string, uint[], string>)__DisassembleCallback).Method,
					Context.DecoderReference.Name,
					Array
				));
			});


			//Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, String>>("Decoder", SwitchTree);
		});
	}
}
