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
		static public BranchResult JP(BranchContext Context, ushort Address) { return new BranchResult(BranchType.FixedDestination, Address); }
		static public BranchResult CALL(BranchContext Context, ushort Address) { return new BranchResult(BranchType.FixedDestination, Address); }
		static public BranchResult SE_n(BranchContext Context, byte X, byte Byte) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2); }
		static public BranchResult SNE_n(BranchContext Context, byte X, byte Byte) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2);}
		static public BranchResult SE_v(BranchContext Context, byte X, byte Y) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2); }
		static public BranchResult SNE_v(BranchContext Context, byte X, byte Y) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2); }
		static public BranchResult JP_addr(BranchContext Context, ushort Address) { return new BranchResult(BranchType.UnknownDestination); }
		static public BranchResult SKP(BranchContext Context, byte X) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2); }
		static public BranchResult SKNP(BranchContext Context, byte X) { return new BranchResult(BranchType.FixedDestination, Context.EndInstructionAddress + 2); }
		static public BranchResult INVALID(BranchContext Context) { return new BranchResult(BranchType.NoJump); }
	}
}
