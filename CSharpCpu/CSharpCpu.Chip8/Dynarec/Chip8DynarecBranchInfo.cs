using CSharpCpu.Cpus;
using CSharpCpu.Cpus.Dynarec;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Chip8.Dynarec
{
	public sealed class Chip8DynarecBranchInfo
	{
		private const AnalyzeType STOP = AnalyzeType.StopAnalyzingCurrentBranch;
		private const AnalyzeType CONTINUE = AnalyzeType.ContinueAnalyzingCurrentBranch;

		private const BranchType FOLLOW = BranchType.FollowAnalyzedAddresses;
		private const BranchType NO_FOLLOW = BranchType.NoFollowAnalyzedAddresses;

		static public BranchResult RET(BranchContext Context) { return new BranchResult(STOP, NO_FOLLOW); }
		static public BranchResult JP(BranchContext Context, ushort Address) { return new BranchResult(CONTINUE, FOLLOW, Address); }
		static public BranchResult CALL(BranchContext Context, ushort Address) { return new BranchResult(CONTINUE, NO_FOLLOW, Address); }
		static public BranchResult SE_n(BranchContext Context, byte X, byte Byte) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult SNE_n(BranchContext Context, byte X, byte Byte) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult SE_v(BranchContext Context, byte X, byte Y) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult SNE_v(BranchContext Context, byte X, byte Y) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult JP_addr(BranchContext Context, ushort Address) { return new BranchResult(CONTINUE, NO_FOLLOW); }
		static public BranchResult SKP(BranchContext Context, byte X) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult SKNP(BranchContext Context, byte X) { return new BranchResult(CONTINUE, FOLLOW, Context.EndInstructionAddress + 2); }
		static public BranchResult OTHER(BranchContext Context) { return new BranchResult(CONTINUE, NO_FOLLOW); }
	}
}
