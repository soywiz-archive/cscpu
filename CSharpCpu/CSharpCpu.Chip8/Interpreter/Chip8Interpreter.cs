using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public class Chip8Interpreter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void SYS(CpuContext Context, ushort Address)
		{
			Context.Syscall.Call(Context, Address);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void JP(CpuContext Context, ushort Address)
		{
			Context.PC = Address;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Context"></param>
		/// <param name="Address"></param>
		static public void CALL(CpuContext Context, ushort Address)
		{
			Context.CallStack.Push(Address);
			JP(Context, Address);
		}

		//Instruction("3xnn", "SE", "%vx, %byte"),
		//Instruction("4xnn", "SNE", "%vx, %byte"),
		//Instruction("5xy0", "SE", "%vx, %vy"),
		//Instruction("6xnn", "LD", "%vx, %byte"),
		//Instruction("7xnn", "ADD", "%vx, %byte"),
		//Instruction("8xy0", "LD", "%vx, %vy"),
		//Instruction("8xy1", "OR", "%vx, %vy"),
		//Instruction("8xy2", "AND", "%vx, %vy"),
		//Instruction("8xy3", "XOR", "%vx, %vy"),
		//Instruction("8xy4", "ADD", "%vx, %vy"),
		//Instruction("8xy5", "SUB", "%vx, %vy"),
		//Instruction("8xy6", "SHR", "%vx, %vy"),
		//Instruction("8xy7", "SUBN", "%vx, %vy"),
		//Instruction("8xyE", "SHL", "%vx, %vy"),
		//Instruction("9xy0", "SNE", "%vx, %vy"),
		//Instruction("Annn", "LD", "I, %addr"),
		//Instruction("Bnnn", "JP", "V0, %addr"),
		//Instruction("Cxnn", "RND", "%vx, %byte"),
		//Instruction("Dxyn", "DRW", "%vx, %vy, %nibble"),
		//Instruction("Ex9E", "SKP", "%vx"),
		//Instruction("ExA1", "SKNP", "%vx"),
		//Instruction("Fx07", "LD", "%vx, DT"),
		//Instruction("Fx0A", "LD", "%vx, K"),
		//Instruction("Fx15", "LD", "DT, %vx"),
		//Instruction("Fx18", "LD", "ST, %vx"),
		//Instruction("Fx1E", "ADD", "I, %vx"),
		//Instruction("Fx29", "LD", "F, %vx"),
		//Instruction("Fx33", "LD", "B, %vx"),
		//Instruction("Fx55", "LD", "[I], %vx"),
		//Instruction("Fx65", "LD", "%vx, [I]"),
	}
}
