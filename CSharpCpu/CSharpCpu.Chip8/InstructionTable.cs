using CSharpCpu.Decoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public class InstructionTable : IInstructionTable 
	{
		static InstructionFlags InstructionFlags = new InstructionFlags()
		{
			DecodeSize = 2,
			FixedSize = true,
		};

		public static InstructionInfo[] Instructions = new[]
		{
			//Instruction("00E0", "CLS", ""),
			//Instruction("00EE", "RET", ""),
			Instruction("0nnn", "SYS", "%addr"),
			Instruction("1nnn", "JP", "%addr"),
			Instruction("2nnn", "CALL", "%addr"),
			Instruction("3xnn", "SE", "%vx, %byte"),
			Instruction("4xnn", "SNE", "%vx, %byte"),
			Instruction("5xy0", "SE", "%vx, %vy"),
			Instruction("6xnn", "LD", "%vx, %byte"),
			Instruction("7xnn", "ADD", "%vx, %byte"),
			Instruction("8xy0", "LD", "%vx, %vy"),
			Instruction("8xy1", "OR", "%vx, %vy"),
			Instruction("8xy2", "AND", "%vx, %vy"),
			Instruction("8xy3", "XOR", "%vx, %vy"),
			Instruction("8xy4", "ADD", "%vx, %vy"),
			Instruction("8xy5", "SUB", "%vx, %vy"),
			Instruction("8xy6", "SHR", "%vx, %vy"),
			Instruction("8xy7", "SUBN", "%vx, %vy"),
			Instruction("8xyE", "SHL", "%vx, %vy"),
			Instruction("9xy0", "SNE", "%vx, %vy"),
			Instruction("Annn", "LD", "I, %addr"),
			Instruction("Bnnn", "JP", "V0, %addr"),
			Instruction("Cxnn", "RND", "%vx, %byte"),
			Instruction("Dxyn", "DRW", "%vx, %vy, %nibble"),
			Instruction("Ex9E", "SKP", "%vx"),
			Instruction("ExA1", "SKNP", "%vx"),
			Instruction("Fx07", "LD", "%vx, DT"),
			Instruction("Fx0A", "LD", "%vx, K"),
			Instruction("Fx15", "LD", "DT, %vx"),
			Instruction("Fx18", "LD", "ST, %vx"),
			Instruction("Fx1E", "ADD", "I, %vx"),
			Instruction("Fx29", "LD", "F, %vx"),
			Instruction("Fx33", "LD", "B, %vx"),
			Instruction("Fx55", "LD", "[I], %vx"),
			Instruction("Fx65", "LD", "%vx, [I]"),
		};

		InstructionFlags IInstructionTable.InstructionFlags
		{
			get { return InstructionTable.InstructionFlags; }
		}

		static private InstructionInfo Instruction(string Opcode, string Mnemonic, string Format)
		{
			ushort Mask = 0xFFFF;
			ushort Value = 0x0000;
			var VarReferenceList = new List<VarReference>();

			for (int n = 0; n < Opcode.Length; n++)
			{
				uint Shift = (uint)((Opcode.Length - n - 1) * 4);
				switch (Opcode[n])
				{
					case 'x':
					case 'y':
						Mask <<= 4;
						Value <<= 4;
						VarReferenceList.Add(new VarReference("" + Opcode[n], Shift, 0xF));
						break;
					case 'n':
						switch (Opcode.Substring(n))
						{
							case "nnn":
								n += 2;
								Mask <<= 12;
								Value <<= 12;
								VarReferenceList.Add(new VarReference("nnn", Shift, 0xFFF));
								break;
							case "nn":
								n += 1;
								Mask <<= 8;
								Value <<= 8;
								VarReferenceList.Add(new VarReference("nn", Shift, 0xFF));
								break;
							case "n":
								n += 0;
								Mask <<= 4;
								Value <<= 4;
								VarReferenceList.Add(new VarReference("n", Shift, 0xF));
								break;
							default:
								throw (new Exception("Unexpected"));
						}
						break;
					default:
						Mask <<= 4; Mask |= 0xF;
						Value <<= 4; Value |= Convert.ToUInt16("" + Opcode[n], 16);
						break;
				}
			}

			return new InstructionInfo(Mnemonic, Format, new MaskDataVars(Mask, Value, VarReferenceList.ToArray()));
		}
	}
}
