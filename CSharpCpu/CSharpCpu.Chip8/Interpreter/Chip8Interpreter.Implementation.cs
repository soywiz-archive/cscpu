using CSharpCpu.Cpus.Chip8;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8.Interpreter
{
	public sealed partial class Chip8Interpreter
	{
		/// <summary>
		/// 0NNN: Calls RCA 1802 program at address NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void SYS(CpuContext Context, ushort Address)
		{
			switch (Address)
			{
				// Clears the screen.
				case 0x0E0: Context.Display.Clear(); break;
				// Returns from a subroutine.
				case 0x0EE: Context.PC = Context.CallStack.Pop(); break;
				default: Context.Syscall.Call(Context, Address); break;
			}
		}

		/// <summary>
		/// 1NNN: Jumps to address NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void JP(CpuContext Context, ushort Address)
		{
			Context.PC = Address;
		}

		/// <summary>
		/// 2NNN: Calls subroutine at NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void CALL(CpuContext Context, ushort Address)
		{
			Context.CallStack.Push(Context.PC);
			JP(Context, Address);
		}

		/// <summary>
		/// 3XNN : Skips the next instruction if VX equals NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public void SE_n(CpuContext Context, byte X, byte Byte)
		{
			if (Context.V[X] == Byte) Context.PC += 2;
		}

		/// <summary>
		/// 4XNN : Skips the next instruction if VX doesn't equal NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public void SNE_n(CpuContext Context, byte X, byte Byte)
		{
			if (Context.V[X] != Byte) Context.PC += 2;
		}

		/// <summary>
		/// 5XY0: Skips the next instruction if VX equals VY.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		static public void SE_v(CpuContext Context, byte X, byte Y)
		{
			if (Context.V[X] == Context.V[Y]) Context.PC += 2;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		static public void SNE_v(CpuContext Context, byte X, byte Y)
		{
			if (Context.V[X] != Context.V[Y]) Context.PC += 2;
		}

		/// <summary>
		/// 6XNN: Sets VX to NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public void LD_n(CpuContext Context, byte X, byte Byte)
		{
			Context.V[X] = Byte;
		}

		/// <summary>
		/// 8xy0: LD Vx, Vy
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Y"></param>
		static public void LD_v(CpuContext Context, byte X, byte Y)
		{
			Context.V[X] = Context.V[Y];
		}

		static private void _ADD(CpuContext Context, byte X, byte Value)
		{
			Context.V[15] = (byte)(((Context.V[X] + Value) > 255) ? 1 : 0);
			Context.V[X] += Value;
		}

		static private void _SUB(CpuContext Context, byte X, byte Value)
		{
			Context.V[15] = (byte)(((Context.V[X] > Value)) ? 1 : 0);
			Context.V[X] -= Value;
		}

		static public void ADD_n(CpuContext Context, byte X, byte Byte) {
			_ADD(Context, X, Byte);
			//Context.V[X] += Byte;
		}
		static public void ADD_v(CpuContext Context, byte X, byte Y) { _ADD(Context, X, Context.V[Y]); }
		static public void OR(CpuContext Context, byte X, byte Y) { Context.V[X] |= Context.V[Y]; }
		static public void AND(CpuContext Context, byte X, byte Y) { Context.V[X] &= Context.V[Y]; }
		static public void XOR(CpuContext Context, byte X, byte Y) { Context.V[X] ^= Context.V[Y]; }
		static public void SUB(CpuContext Context, byte X, byte Y) { _SUB(Context, X, Context.V[Y]); }
		
		static public void SHR(CpuContext Context, byte X, byte Y) { throw (new NotImplementedException()); }
		static public void SUBN(CpuContext Context, byte X, byte Y) { throw (new NotImplementedException()); }
		static public void SHL(CpuContext Context, byte X, byte Y) { throw (new NotImplementedException()); }

		/// <summary>
		/// Annn: Sets I to the address NNN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void LD_addr(CpuContext Context, ushort Address)
		{
			Context.I = Address;
		}

		/// <summary>
		/// Bnnn: Jumps to the address NNN plus V0.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void JP_addr(CpuContext Context, ushort Address)
		{
			Context.PC = (ushort)(Address + Context.V[0]);
		}

		/// <summary>
		/// Cxnn: Sets VX to a random number and NN.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		/// <param name="Byte"></param>
		static public void RND(CpuContext Context, byte X, byte Byte)
		{
			Context.V[X] = (byte)(Context.Random.Next() & Byte);
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
		static public void DRW(CpuContext Context, byte X, byte Y, byte Nibble)
		{
			//Console.WriteLine("Draw({0}, {1}, {2})", Context.V[X], Context.V[Y], Nibble);
			Context.Display.Draw(ref Context.I, ref Context.V[15], Context.Memory, Context.V[X], Context.V[Y], Nibble);
		}

		/// <summary>
		/// Ex9E: Skips the next instruction if the key stored in VX is pressed.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void SKP(CpuContext Context, byte X)
		{
			if (Context.Controller.IsPressed(Context.V[X])) Context.PC += 2;
		}

		/// <summary>
		/// ExA1: Skips the next instruction if the key stored in VX isn't pressed.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void SKNP(CpuContext Context, byte X)
		{
			if (!Context.Controller.IsPressed(Context.V[X])) Context.PC += 2;
		}

		/// <summary>
		/// Fx07: LD Vx, DT
		/// Set Vx = delay timer value
		/// The value of DT is placed into Vx.
		/// Sets VX to the value of the delay timer.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_vx_dt(CpuContext Context, byte X)
		{
			//Console.WriteLine("{0}", Context.DelayTimer.Value);
			Context.V[X] = Context.DelayTimer.Value;
		}

		/// <summary>
		/// Fx0A: Wait for a key press, store the value of the key in Vx.
		/// All execution stops until a key is pressed, then the value of that key is stored in Vx
		/// A key press is awaited, and then stored in VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_vx_k(CpuContext Context, byte X)
		{
			var Ret = Context.Controller.GetPressMask();
			if (Ret.HasValue)
			{
				Context.V[X] = Ret.Value;
			}
			else
			{
				Context.PC -= 2;
			}
		}

		/// <summary>
		/// Fx15: Sets the delay timer to VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_dt_vx(CpuContext Context, byte X)
		{
			Context.DelayTimer.Value = Context.V[X];
		}

		/// <summary>
		/// Fx18: Sets the sound timer to VX.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_st_vx(CpuContext Context, byte X)
		{
			Context.SoundTimer.Value = Context.V[X];
		}

		/// <summary>
		/// Fx1E: Adds VX to I.
		/// VF is set to 1 when range overflow (I+VX>0xFFF), and 0 when there isn't.
		/// This is undocumented feature of the Chip-8 and used by Spacefight 2019! game.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void ADD_i_vx(CpuContext Context, byte X)
		{
			Context.I += Context.V[X];
		}

		/// <summary>
		/// Fx29: Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_f_vx(CpuContext Context, byte X)
		{
			Context.I = (ushort)(Context.V[X] * 5);
		}

		/// <summary>
		/// Fx33: Stores the Binary-coded decimal representation of VX, with the most significant of three digits at the address in I,
		/// the middle digit at I plus 1, and the least significant digit at I plus 2. (In other words, take the decimal
		/// representation of VX, place the hundreds digit in memory at location in I, the tens digit at location I+1, and
		/// the ones digit at location I+2.)
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_b_vx(CpuContext Context, byte X)
		{
			Context.Memory.Write1((ushort)(Context.I + 0), (byte)((Context.V[X] / 100) % 10));
			Context.Memory.Write1((ushort)(Context.I + 1), (byte)((Context.V[X] / 10) % 10));
			Context.Memory.Write1((ushort)(Context.I + 2), (byte)((Context.V[X] / 1) % 10));
		}

		/// <summary>
		/// Fx55: Stores V0 to VX in memory starting at address I.
		/// On the original interpreter, when the operation is done, I=I+X+1.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_Iptr_vx(CpuContext Context, byte X)
		{
			for (int n = 0; n < X; n++)
			{
				Context.Memory.Write1(Context.I++, Context.V[n]);
			}
		}

		/// <summary>
		/// Fx65: Fills V0 to VX with values from memory starting at address I.
		/// On the original interpreter, when the operation is done, I=I+X+1.
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="X"></param>
		static public void LD_vx_Iptr(CpuContext Context, byte X)
		{
			for (int n = 0; n < X; n++)
			{
				Context.V[n] = Context.Memory.Read1(Context.I++);
			}
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		static public void INVALID(CpuContext Context)
		{
			throw (new Exception("Invalid instruction!"));
		}
	}
}
