using CSharpCpu.Cpus;
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

namespace CSharpCpu.Z80.Disassembler
{
	public class Z80Disassembler
	{
		static private AstGenerator ast = AstGenerator.Instance;

		private IMemory1 Memory;
		public ushort Address;
		private Func<Func<uint>, string> Decoder;

		public Z80Disassembler(IMemory1 Memory)
		{
			this.Memory = Memory;
			this.Decoder = LazyDecoder.Value;
		}

		public string DecodeNext()
		{
			return this.Decoder(() => Memory.Read1(this.Address++));
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

		static private Lazy<Func<Func<uint>, string>> LazyDecoder = new Lazy<Func<Func<uint>,string>>(() => 
		{
			var Ins = CSharpCpu.Cpus.Z80.InstructionTable.Instructions;

			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<string, InstructionInfo>(Ins, (Context) =>
			{
				if (Context.DecoderReference == null) return ast.Return("Unknown");

				var Array = new AstNodeExprNewArray(typeof(uint));

				MatchArgument.Replace(Context.DecoderReference.Name, (Match) =>
				{
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

				return ast.Return(ast.CallStatic(
					((Func<string, uint[], string>)__DisassembleCallback).Method,
					Context.DecoderReference.Name,
					Array
				));
			});


			//Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<Func<uint>, String>>("Decoder", SwitchTree);
		});
	}
}
