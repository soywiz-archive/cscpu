using CSharpCpu.Cpus.Chip8;
using CSharpCpu.Cpus.Dynarec;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Chip8.Dynarec
{
	public class DynarecContextChip8 : DynarecContext
	{
		public AstNodeExpr GetCpuContext()
		{
			return ast.Argument<CpuContext>(0, "CpuContext");
		}

		public AstNodeExpr GetCallStack()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.CallStack));
		}

		public AstNodeExpr GetController()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.Controller));
		}

		public AstNodeExpr GetDisplay()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.Display));
		}

		public AstNodeExprLValue GetDelayTimerValue()
		{
			return ast.FieldAccess(ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.DelayTimer)), ILFieldInfo.GetFieldInfo(() => Chip8Timer._NullInstance.Value));
		}

		public AstNodeExprLValue GetSoundTimerValue()
		{
			return ast.FieldAccess(ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.SoundTimer)), ILFieldInfo.GetFieldInfo(() => Chip8Timer._NullInstance.Value));
		}

		public AstNodeExprLValue GetI()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.I));
		}

		public AstNodeExprLValue GetPC()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.PC));
		}

		public AstNodeExprLValue GetRegister(byte Index)
		{
			return ast.FieldAccess(GetCpuContext(), "V" + Index);
		}

		public AstNodeExpr GetCallForAddress(AstNodeExpr Address)
		{
			return ast.CallInstance(GetCpuContext(), (Func<uint, Action<CpuContext>>)CpuContext._NullInstance.GetDelegateForAddress, ast.Cast<uint>(Address));
		}

		public AstNodeExprLValue GetInstructionCount()
		{
			return ast.FieldAccess(GetCpuContext(), ILFieldInfo.GetFieldInfo(() => CpuContext._NullInstance.InstructionCount));
		}

		internal AstNodeStm GetDynarecTick()
		{
			return ast.Statement(ast.CallInstance(GetCpuContext(), (Action)CpuContext._NullInstance.DynarecTick));
		}
	}
}
