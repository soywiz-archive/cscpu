﻿using CSharpCpu.Cpus;
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

		static public Lazy<Func<SwitchReadWordDelegate, InstructionDynarecInfo>> DecodeInstructionInfo = new Lazy<Func<SwitchReadWordDelegate, InstructionDynarecInfo>>(() => CreateDecodeInstructionInfo());
		static public Lazy<Func<SwitchReadWordDelegate, BranchContext, BranchResult>> DecodeBranchContext = new Lazy<Func<SwitchReadWordDelegate, BranchContext, BranchResult>>(() => CreateDecodeBranchContext());
		static public Lazy<Func<SwitchReadWordDelegate, DynarecContextChip8, DynarecResult>> DecodeDynare = new Lazy<Func<SwitchReadWordDelegate, DynarecContextChip8, DynarecResult>>(() => CreateDecodeDynarec());

		static private Func<SwitchReadWordDelegate, DynarecContextChip8, DynarecResult> CreateDecodeDynarec()
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<DynarecResult, InstructionInfo>(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Return(ast.Null<DynarecResult>());
				}

				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;

				var InstructionIndex = Array.IndexOf(InstructionTable.Instructions, InstructionInfo);
				if (InstructionIndex < 0) throw (new Exception("Can't find instruction"));

				var Parameters = InstructionTable.ParseParameters(InstructionInfo, Scope);
				Parameters.Insert(0, ast.Argument<DynarecContextChip8>(1));

				//Parameters.Add(ast.Argument<CpuContext>(1));

				var MethodInfo = typeof(Chip8DynarecImplementation).GetMethod(InstructionInfo.Name);

				if (MethodInfo == null)
				{
					MethodInfo = ((Func<DynarecContextChip8, DynarecResult>)Chip8DynarecImplementation.INVALID).Method;
					return ast.Return(ast.CallStatic(MethodInfo, Parameters[0]));
				}
				else
				{
					return ast.Return(ast.CallStatic(MethodInfo, Parameters.ToArray()));
				}
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));
			//Console.WriteLine(GeneratorIL.GenerateToString<Func<SwitchReadWordDelegate, InstructionDynarecInfo>>(SwitchTree));
			//
			//Console.ReadKey();

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, DynarecContextChip8, DynarecResult>>("DecodeDynarec", SwitchTree);
		}

		static private Func<SwitchReadWordDelegate, BranchContext, BranchResult> CreateDecodeBranchContext()
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<BranchResult, InstructionInfo>(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Return(ast.Null<BranchResult>());
				}

				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;

				var InstructionIndex = Array.IndexOf(InstructionTable.Instructions, InstructionInfo);
				if (InstructionIndex < 0) throw (new Exception("Can't find instruction"));

				var Parameters = InstructionTable.ParseParameters(InstructionInfo, Scope);
				Parameters.Insert(0, ast.Argument<BranchContext>(1));

				//Parameters.Add(ast.Argument<CpuContext>(1));

				var MethodInfo = typeof(Chip8DynarecBranchInfo).GetMethod(InstructionInfo.Name);

				if (MethodInfo == null)
				{
					MethodInfo = ((Func<BranchContext, BranchResult>)Chip8DynarecBranchInfo.OTHER).Method;
					return ast.Return(ast.CallStatic(MethodInfo, Parameters[0]));
				}
				else
				{
					return ast.Return(ast.CallStatic(MethodInfo, Parameters.ToArray()));
				}
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));
			//Console.WriteLine(GeneratorIL.GenerateToString<Func<SwitchReadWordDelegate, InstructionDynarecInfo>>(SwitchTree));
			//
			//Console.ReadKey();

			return GeneratorIL.GenerateDelegate<GeneratorIL, Func<SwitchReadWordDelegate, BranchContext, BranchResult>>("DecodeBranchContext", SwitchTree);
		}

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

				var Parameters = InstructionTable.ParseParameters(InstructionInfo, Scope);

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

		static public Action<CpuContext> CreateDynarecFunction(IMemory2 Memory, uint PC)
		{
			return GeneratorIL.GenerateDelegate<GeneratorIL, Action<CpuContext>>(String.Format("Method_{0:X8}", PC), AnalyzeFunction(Memory, PC));
		}

		static public AstNodeStm AnalyzeFunction(IMemory2 Memory, uint PC)
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
			var DynarecContext = new DynarecContextChip8();
			Queue<uint> BranchesToAnalyze = new Queue<uint>();
			var MinPC = uint.MaxValue;
			var MaxPC = uint.MinValue;
			BranchesToAnalyze.Enqueue(PC);

			var DecodeInstructionInfo = Chip8Dynarec.DecodeInstructionInfo.Value;
			var DecodeBranchContext = Chip8Dynarec.DecodeBranchContext.Value;
			var DecodeDynarec = Chip8Dynarec.DecodeDynare.Value;

			var AddLabelForPC = (Action<uint>)((_PC) =>
			{
				DynarecContext.PCToLabel[_PC] = AstLabel.CreateLabel(String.Format("Label_{0:X8}", _PC));
			});

			// PASS1: Branch analyzing
			Console.WriteLine("- PASS 1 ------------------------------------");
			while (BranchesToAnalyze.Count > 0)
			{
				PC = BranchesToAnalyze.Dequeue();
				Console.WriteLine("Analyzing: {0:X8}", PC);
				AddLabelForPC(PC);

				while (true)
				{
					//Console.ReadKey();

					// Already analyzed.
					if (AnalyzedPC.Contains(PC))
					{
						break;
					}

					AnalyzedPC.Add(PC);

					MinPC = Math.Min(PC, MinPC);
					MaxPC = Math.Min(PC, MaxPC);

					var CurrentPC = PC;
					PC = CurrentPC; var Instruction = DecodeInstructionInfo(Reader);
					var EndPC = PC;

					PC = CurrentPC; var BranchInfo = DecodeBranchContext(Reader, new BranchContext(CurrentPC, EndPC));

					if (Instruction == null)
					{
						//throw (new Exception("Invalid!"));
						Console.WriteLine("{0:X4}: Invalid Instruction", CurrentPC);
					}
					else
					{
						Console.WriteLine("{0:X4}: {1} : {2}", CurrentPC, Instruction.InstructionInfo.Name, BranchInfo.BranchType);
						
						// Must follow jumps.
						if (BranchInfo.FollowJumps)
						{
							foreach (var JumpAddress in BranchInfo.PossibleJumpList)
							{
								Console.WriteLine("{0:X4}: Enqueueing BranchesToAnalyze {1:X4}", CurrentPC, JumpAddress);
								BranchesToAnalyze.Enqueue(JumpAddress);
							}
						}

						// Stop exploring this branch.
						if (!BranchInfo.ContinueAnalyzing)
						{
							break;
						}
					}
				}
			}

			// PASS2: Code generation
			var Ast = ast.Statements();
			Console.WriteLine("- PASS 2 ------------------------------------");
			foreach (var CurrentPC in AnalyzedPC.OrderBy(Item => Item))
			{
				PC = CurrentPC;

				if (DynarecContext.PCToLabel.ContainsKey(PC))
				{
					Ast.AddStatement(ast.Label(DynarecContext.PCToLabel[PC]));
				}

				DynarecContext.CurrentPC = PC;
				PC = CurrentPC; DecodeInstructionInfo(Reader);
				DynarecContext.EndPC = PC;

				PC = CurrentPC; var DynarecResult = DecodeDynarec(Reader, DynarecContext);
				Ast.AddStatement(DynarecResult.AstNodeStm);
			}
			Ast.AddStatement(ast.Return());

			//Console.WriteLine(GeneratorCSharp.GenerateString(Ast));

			return Ast;


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
