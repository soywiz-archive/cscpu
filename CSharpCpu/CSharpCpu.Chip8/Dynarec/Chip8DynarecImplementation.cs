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

		static public DynarecResult RET(DynarecContextChip8 Context)
		{
			return ast.Return();
		}

		/// <summary>
		/// 1NNN: Jumps to address NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public DynarecResult JP(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statement(ast.CallTail(ast.CallDelegate(Context.GetCallForAddress(Address), Context.GetCpuContext())));
		}

		/// <summary>
		/// 2NNN: Calls subroutine at NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public DynarecResult CALL(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statement(ast.CallDelegate(Context.GetCallForAddress(Address), Context.GetCpuContext()));
		}

		/// <summary>
		/// 3XNN : Skips the next instruction if VX equals NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public DynarecResult SE_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "==", Byte),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		/// <summary>
		/// 4XNN : Skips the next instruction if VX doesn't equal NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public DynarecResult SNE_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "!=", Byte),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		/// <summary>
		/// 5XY0: Skips the next instruction if VX equals VY.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		static public DynarecResult SE_v(DynarecContextChip8 Context, byte X, byte Y)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "==", Context.GetRegister(Y)),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		static public DynarecResult SNE_v(DynarecContextChip8 Context, byte X, byte Y)
		{
			return ast.IfElse(
				ast.Binary(Context.GetRegister(X), "!=", Context.GetRegister(Y)),
				ast.GotoAlways(Context.PCToLabel[Context.EndPC + 2])
			);
		}

		/// <summary>
		/// 6XNN: Sets VX to NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public DynarecResult LD_n(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.Assign(Context.GetRegister(X), ast.Immediate(Byte));
		}

		/// <summary>
		/// 8xy0: LD Vx, Vy
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
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

		/// <summary>
		/// Annn: Sets I to the address NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public DynarecResult LD_addr(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Assign(Context.GetI(), Address);
		}

		/// <summary>
		/// Bnnn: Jumps to the address NNN plus V0.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public DynarecResult JP_addr(DynarecContextChip8 Context, ushort Address)
		{
			return ast.Statement(ast.CallTail(ast.CallDelegate(Context.GetCallForAddress(Address + Context.GetRegister(0)), Context.GetCpuContext())));
		}

		/// <summary>
		/// Cxnn: Sets VX to a random number and NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public DynarecResult RND(DynarecContextChip8 Context, byte X, byte Byte)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte, byte>)Chip8InterpreterImplementation.RND, Context.GetCpuContext(), X, Byte));
		}

		/// <summary>
		/// Dxyn: Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
		/// Each row of 8 pixels is read as bit-coded (with the most significant bit of each byte displayed on the left)
		/// starting from memory location I; I value doesn't change after the execution of this instruction.
		/// As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn't happen.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		/// <param name="Nibble"></param>
		static public DynarecResult DRW(DynarecContextChip8 Context, byte X, byte Y, byte Nibble)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte, byte, byte>)Chip8InterpreterImplementation.DRW, Context.GetCpuContext(), X, Y, Nibble));
		}

		/// <summary>
		/// Ex9E: Skips the next instruction if the key stored in VX is pressed.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
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

		/// <summary>
		/// ExA1: Skips the next instruction if the key stored in VX isn't pressed.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
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

		/// <summary>
		/// Fx07: LD Vx, DT
		/// Set Vx = delay timer value
		/// The value of DT is placed into Vx.
		/// Sets VX to the value of the delay timer.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_vx_dt(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetRegister(X), Context.GetDelayTimerValue());
		}

		/// <summary>
		/// Fx0A: Wait for a key press, store the value of the key in Vx.
		/// All execution stops until a key is pressed, then the value of that key is stored in Vx
		/// A key press is awaited, and then stored in VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_vx_k(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_vx_k, Context.GetCpuContext(), X));
		}

		/// <summary>
		/// Fx15: Sets the delay timer to VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_dt_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetDelayTimerValue(), Context.GetRegister(X));
		}

		/// <summary>
		/// Fx18: Sets the sound timer to VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_st_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetSoundTimerValue(), Context.GetRegister(X));
		}

		/// <summary>
		/// Fx1E: Adds VX to I.
		/// VF is set to 1 when range overflow (I+VX>0xFFF), and 0 when there isn't.
		/// This is undocumented feature of the Chip-8 and used by Spacefight 2019! game.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult ADD_i_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetI(), Context.GetI() + Context.GetRegister(X));
		}

		/// <summary>
		/// Fx29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_f_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Assign(Context.GetI(), ast.Cast<ushort>(ast.Cast<int>(Context.GetRegister(X)) * ast.Cast<int>(5)));
		}

		/// <summary>
		/// Fx33: Stores the Binary-coded decimal representation of VX, with the most significant of three digits at the address in I,
		/// the middle digit at I plus 1, and the least significant digit at I plus 2. (In other words, take the decimal
		/// representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and
		/// the ones digit at location I+2.)
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_b_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_b_vx, Context.GetCpuContext(), X));
		}

		/// <summary>
		/// Fx55: Stores V0 to VX in memory starting at address I.
		/// On the original interpreter, when the operation is done, I=I+X+1.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_Iptr_vx(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_Iptr_vx, Context.GetCpuContext(), X));
		}

		/// <summary>
		/// Fx65: Fills V0 to VX with values from memory starting at address I.
		/// On the original interpreter, when the operation is done, I=I+X+1.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public DynarecResult LD_vx_Iptr(DynarecContextChip8 Context, byte X)
		{
			return ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Chip8InterpreterImplementation.LD_vx_Iptr, Context.GetCpuContext(), X));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		static public DynarecResult INVALID(DynarecContextChip8 Context)
		{
			return ast.Throw(ast.New<Exception>("Invalid instruction!"));
		}
	}
}
