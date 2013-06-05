using CSharpCpu.Cpus;
using CSharpCpu.Cpus.Chip8;
using CSharpCpu.Cpus.Dynarec;
using CSharpCpu.Decoder;
using CSharpCpu.Memory;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Chip8.Dynarec
{
	public class Chip8Dynarec
	{
		static private AstGenerator ast = AstGenerator.Instance;

		static public Func<SwitchReadWordDelegate, InstructionInfo> DecodeInstructionInfo = CreateDecodeInstructionInfo();

		static private Func<SwitchReadWordDelegate, InstructionInfo> CreateDecodeInstructionInfo()
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<InstructionInfo, InstructionInfo>(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null) return ast.Return(ast.Null<InstructionInfo>());
				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;
				var InstructionIndex = Array.IndexOf(InstructionTable.Instructions, InstructionInfo);
				if (InstructionIndex < 0) throw (new Exception("Can't find instruction"));
				return ast.Return(ast.ArrayAccess(ast.StaticFieldAccess(() => InstructionTable.Instructions), ast.Immediate((uint)InstructionIndex)));
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));
			//Console.WriteLine(GeneratorIL.GenerateToString<GeneratorIL, Func<SwitchReadWordDelegate, InstructionInfo>>(SwitchTree));
			//
			//Console.ReadKey();

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, InstructionInfo>>("GetInstructionInfo", SwitchTree);
		}

		static public void AnalyzeFunction(IMemory2 Memory, ushort AnalyzeAddress)
		{
			Console.WriteLine(AnalyzeAddress);
			var Reader = (SwitchReadWordDelegate)(() =>
			{
				var Ret = (uint)Memory.Read2(AnalyzeAddress);
				Console.WriteLine("{0:X4}: {1:X4}", AnalyzeAddress, Ret);
				AnalyzeAddress += 2;
				return Ret;
			});

			//Console.ReadKey();
			Console.WriteLine(DecodeInstructionInfo(Reader).InstructionType);
			Console.WriteLine(DecodeInstructionInfo(Reader).InstructionType);
			Console.WriteLine(DecodeInstructionInfo(Reader).InstructionType);
			Console.WriteLine(DecodeInstructionInfo(Reader).InstructionType);
			Console.WriteLine(DecodeInstructionInfo(Reader).InstructionType);
			//DecodeInstructionInfo();
		}
	}
}
