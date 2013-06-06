#define NATIVE_CALLS
//#define NATIVE_JUMPS

using CSharpCpu.Chip8.Interpreter;
using CSharpCpu.Cpus;
using CSharpCpu.Cpus.Chip8;
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
	public sealed class Chip8DynarecImplementation
	{
		static private AstGenerator ast = AstGenerator.Instance;

		//static public DynarecResult SYS(DynarecContext DynarecContext, ushort Address)
		//{
		//	return ast.Statement();
		//}

		static public DynarecResult CLS(DynarecContextChip8 Context)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext>)Chip8InterpreterImplementation.CLS, Context.GetCpuContext()));
		}

		static public DynarecResult CALL(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statements(
#if NATIVE_CALLS
				ast.Statement(ast.CallDelegate(Context.GetCallForAddress(Address), Context.GetCpuContext()))
#else
				ast.Statement(ast.CallInstance(Context.GetCallStack(), (Action<uint>)((new Stack<uint>()).Push), Context.EndPC)),
				ast.Statements(ast.Assign(Context.GetPC(), Address)),
				ast.Return()
#endif
			);
		}

		static public DynarecResult RET(DynarecContextChip8 Context)
		{
			return ast.Statements(
#if !NATIVE_CALLS
				ast.Assign(Context.GetPC(), ast.Cast<ushort>(ast.CallInstance(Context.GetCallStack(), (Func<uint>)((new Stack<uint>()).Pop)))),
#endif
ast.Return()
			);
		}

		static public DynarecResult JP(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statements(
#if NATIVE_JUMPS
ast.Statement(ast.CallTail(ast.CallDelegate(Context.GetCallForAddress(Address), Context.GetCpuContext()))),
#else
				ast.Statements(ast.Assign(Context.GetPC(), Address)),
#endif
				ast.Return()
			);
		}

		static public DynarecResult JP_addr(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statements(
#if NATIVE_JUMPS
ast.Statement(ast.CallTail(ast.CallDelegate(Context.GetCallForAddress(Address + Context.GetRegister(0)), Context.GetCpuContext()))),
#else
				ast.Statements(ast.Assign(Context.GetPC(), Address + Context.GetRegister(0))),
#endif
 ast.Return()
			);
		}

		static public DynarecResult SE_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "==", Byte),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult SNE_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "!=", Byte),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult SE_v(DynarecContextChip8 Context, byte X, byte Y)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "==", Context.GetRegister(Y)),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult SNE_v(DynarecContextChip8 Context, byte X, byte Y)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "!=", Context.GetRegister(Y)),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult LD_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.Assign(Context.GetRegister(X), ast.Immediate(Byte));
		}

		static public DynarecResult LD_v(DynarecContextChip8 Context, byte X, byte Y)
		{
			return ast.Assign(Context.GetRegister(X), Context.GetRegister(Y));
		}

		static private DynarecResult _ADD(DynarecContextChip8 Context, byte X, AstNodeExpr Value)
		{
			return ast.Statements(
				ast.Assign(Context.GetRegister(15), ast.Cast<byte>(ast.Binary((ast.Cast<uint>(Context.GetRegister(X)) + ast.Cast<uint>(Value)), ">", 255))),
				ast.Assign(Context.GetRegister(X), Context.GetRegister(X) + Value)
			);
		}

		static private DynarecResult _SUB(DynarecContextChip8 Context, byte X, AstNodeExpr Value)
		{
			return ast.Statements(
				ast.Assign(Context.GetRegister(15), ast.Binary((Context.GetRegister(X)), ">", Value)),
				ast.Assign(Context.GetRegister(X), Context.GetRegister(X) - Value)
			);
		}


		static private DynarecResult _BinaryOp(DynarecContextChip8 Context, byte X, string Op, byte Y)
		{
			return ast.Assign(Context.GetRegister(X), ast.Binary(Context.GetRegister(X), Op, Context.GetRegister(Y)));
		}
		static public DynarecResult ADD_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return _ADD(Context, X, Byte);
			//Context.V[X] += Byte;
		}
		static public DynarecResult ADD_v(DynarecContextChip8 Context, byte X, byte Y) { return _ADD(Context, X, Context.GetRegister(Y)); }
		static public DynarecResult OR(DynarecContextChip8 Context, byte X, byte Y) { return _BinaryOp(Context, X, "|", Y); }
		static public DynarecResult AND(DynarecContextChip8 Context, byte X, byte Y) { return _BinaryOp(Context, X, "&", Y); }
		static public DynarecResult XOR(DynarecContextChip8 Context, byte X, byte Y) { return _BinaryOp(Context, X, "^", Y); }
		static public DynarecResult SUB(DynarecContextChip8 Context, byte X, byte Y) { return _SUB(Context, X, Context.GetRegister(Y)); }

		static public DynarecResult SHR(DynarecContextChip8 Context, byte X, byte Y) { throw (new NotImplementedException()); }
		static public DynarecResult SUBN(DynarecContextChip8 Context, byte X, byte Y) { throw (new NotImplementedException()); }
		static public DynarecResult SHL(DynarecContextChip8 Context, byte X, byte Y) { throw (new NotImplementedException()); }

		static public DynarecResult LD_addr(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Assign(Context.GetI(), Address);
		}

		static public DynarecResult RND(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte, byte>)Chip8InterpreterImplementation.RND, Context.GetCpuContext(), X, Byte));
		}

		static public DynarecResult DRW(DynarecContextChip8 Context, byte X, byte Y, byte Nibble)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte, byte, byte>)Chip8InterpreterImplementation.DRW, Context.GetCpuContext(), X, Y, Nibble));
		}

		static public DynarecResult SKP(DynarecContextChip8 Context, byte X)
		{
			return ast.IfElse(
				ast.CallInstance(
					Context.GetController(),
					typeof(IController).GetMethod("IsPressed"),
					Context.GetRegister(X)
				),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult SKNP(DynarecContextChip8 Context, byte X)
		{
			return ast.IfElse(
				ast.Unary("!", ast.CallInstance(
					Context.GetController(),
					typeof(IController).GetMethod("IsPressed"),
					Context.GetRegister(X)
				)),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		static public DynarecResult LD_vx_dt(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetRegister(X), Context.GetDelayTimerValue());
		}

		static public DynarecResult LD_vx_k(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_vx_k, Context.GetCpuContext(), X));
		}

		static public DynarecResult LD_dt_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetDelayTimerValue(), Context.GetRegister(X));
		}

		static public DynarecResult LD_st_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetSoundTimerValue(), Context.GetRegister(X));
		}

		static public DynarecResult ADD_i_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetI(), Context.GetI() + Context.GetRegister(X));
		}

		static public DynarecResult LD_f_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetI(), ast.Cast<ushort>(ast.Cast<int>(Context.GetRegister(X)) * ast.Cast<int>(5)));
		}

		static public DynarecResult LD_b_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_b_vx, Context.GetCpuContext(), X));
		}

		static public DynarecResult LD_Iptr_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_Iptr_vx, Context.GetCpuContext(), X));
		}

		static public DynarecResult LD_vx_Iptr(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_vx_Iptr, Context.GetCpuContext(), X));
		}

		static public DynarecResult INVALID(DynarecContextChip8 Context)
		{
			return ast.Throw(ast.New<Exception>("Invalid instruction!"));
		}
	}
}
