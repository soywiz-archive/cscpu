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

		static public Func<SwitchReadWordDelegate, InstructionDynarecInfo> DecodeInstructionInfo = CreateDecodeInstructionInfo();

		static private Func<SwitchReadWordDelegate, InstructionDynarecInfo> CreateDecodeInstructionInfo()
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<InstructionDynarecInfo, InstructionInfo>(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Return(ast.Null<InstructionDynarecInfo>());
					//return ast.Throw(ast.New<Exception>("Trying to decode invalid function"));
				}
				
				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;

				var InstructionIndex = Array.IndexOf(InstructionTable.Instructions, InstructionInfo);
				if (InstructionIndex < 0) throw (new Exception("Can't find instruction"));

				var Parameters = InstructionTable.ParseParameters(InstructionInfo, Scope, false);

				return ast.Return(
					ast.New<InstructionDynarecInfo>(
						ast.ArrayAccess(ast.StaticFieldAccess(() => InstructionTable.Instructions), ast.Immediate((uint)InstructionIndex)),
						ast.NewArray<uint>(Parameters.Select(Item => ast.Cast<uint>(Item)).ToArray())
					)
				);
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));
			//Console.WriteLine(GeneratorIL.GenerateToString<Func<SwitchReadWordDelegate, InstructionDynarecInfo>>(SwitchTree));
			//
			//Console.ReadKey();

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, InstructionDynarecInfo>>("GetInstructionDynarecInfo", SwitchTree);
		}

		static public void AnalyzeFunction(IMemory2 Memory, uint PC)
		{
			Console.WriteLine(PC);
			var Reader = (SwitchReadWordDelegate)(() =>
			{
				var Ret = (uint)Memory.Read2(PC);
				//Console.WriteLine("{0:X4}: {1:X4}", PC, Ret);
				PC += 2;
				return Ret;
			});

			HashSet<uint> AnalyzedPC = new HashSet<uint>();
			Queue<uint> BranchesToAnalyze = new Queue<uint>();
			BranchesToAnalyze.Enqueue(PC);

			while (BranchesToAnalyze.Count > 0)
			{
				PC = BranchesToAnalyze.Dequeue();
				Console.WriteLine("Analyzing: {0:X8}", PC);

				while (true)
				{
					// Already analyzed.
					if (AnalyzedPC.Contains(PC))
					{
						break;
					}

					AnalyzedPC.Add(PC);
					uint CurrentPC = PC;
					var Instruction = DecodeInstructionInfo(Reader);

					if (Instruction == null)
					{
						//throw (new Exception("Invalid!"));
						Console.WriteLine("{0:X4}: Invalid Instruction", CurrentPC);
					}
					else
					{
						Console.WriteLine("{0:X4}: {1}", CurrentPC, Instruction);
						if (Instruction.InstructionInfo.IsJump)
						{
							Console.WriteLine("{0:X4}: Enqueueing BranchesToAnalyze {1:X4}", CurrentPC, Instruction.Params[0]);
							BranchesToAnalyze.Enqueue(Instruction.Params[0]);
							//throw(new Exception("aaa"));
						}

						// Stop exploring.
						if (Instruction.InstructionInfo.IsStopAnalyzing)
						{
							break;
						}
					}
				}
			}

			//Console.ReadKey();
			//Console.WriteLine();
			//Console.WriteLine(DecodeInstructionInfo(Reader));
			//Console.WriteLine(DecodeInstructionInfo(Reader));
			//Console.WriteLine(DecodeInstructionInfo(Reader));
			//Console.WriteLine(DecodeInstructionInfo(Reader));
			//DecodeInstructionInfo();
		}
	}
}
