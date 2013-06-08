using CSharpCpu.Cpus;
using CSharpCpu.Cpus.Z80;
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
		private Func<SwitchReadWordDelegate, string> Decoder;

		public Z80Disassembler(IMemory1 Memory)
		{
			this.Memory = Memory;
			this.Decoder = LazyDecoder.Value;
		}

		public string DecodeAt(ushort PC)
		{
			this.Address = PC;
			return DecodeNext();
		}

		public string DecodeNext()
		{
			return this.Decoder(() => Memory.Read1(this.Address++));
		}

		static public string __DisassembleCallback(string Key, uint[] _Values)
		{
			var Values = new Queue<uint>(_Values);

			return InstructionTable.MatchArgument.Replace(Key, (Match) =>
			{
				if (Values.Count == 0) return Match.Value;
				return String.Format("${0:X}", Values.Dequeue());
			});
		}

		static private Lazy<Func<SwitchReadWordDelegate, string>> LazyDecoder = new Lazy<Func<SwitchReadWordDelegate, string>>(() => 
		{
			var Ins = CSharpCpu.Cpus.Z80.InstructionTable.Instructions;

			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<string, InstructionInfo>(Ins, (Context) =>
			{
				if (Context.DecoderReference == null) return ast.Return("Unknown");

				var Parameters = InstructionTable.ParseParameters(Context.DecoderReference, Context.Scope);
				var Array = new AstNodeExprNewArray(typeof(uint), Parameters.ToArray());

				return ast.Return(ast.CallStatic(
					((Func<string, uint[], string>)__DisassembleCallback).Method,
					Context.DecoderReference.Name + " " + Context.DecoderReference.Format,
					Array
				));
			});


			//Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, String>>("Decoder", SwitchTree);
		});
	}
}
