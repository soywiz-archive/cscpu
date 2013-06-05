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

		private static InstructionType NORMAL = InstructionType.Normal;
		private static InstructionType JUMP_SOMETIMES = InstructionType.Jump;
		private static InstructionType JUMP_ALWAYS = InstructionType.JumpAlways;
		private static InstructionType CALL = InstructionType.Call;
		private static InstructionType RETURN = InstructionType.Return;

		public static InstructionInfo[] Instructions = new[]
		{
			Instruction("00E0", "CLS", "", NORMAL),
			Instruction("00EE", "RET", "", RETURN),
			//Instruction("0nnn", "SYS", "%addr", NORMAL),
			Instruction("1nnn", "JP", "%addr", JUMP_ALWAYS),
			Instruction("2nnn", "CALL", "%addr", CALL),
			Instruction("3xnn", "SE_n", "%vx, %byte", JUMP_SOMETIMES),
			Instruction("4xnn", "SNE_n", "%vx, %byte", JUMP_SOMETIMES),
			Instruction("5xy0", "SE_v", "%vx, %vy", JUMP_SOMETIMES),
			Instruction("6xnn", "LD_n", "%vx, %byte", NORMAL),
			Instruction("7xnn", "ADD_n", "%vx, %byte", NORMAL),
			Instruction("8xy0", "LD_v", "%vx, %vy", NORMAL),
			Instruction("8xy1", "OR", "%vx, %vy", NORMAL),
			Instruction("8xy2", "AND", "%vx, %vy", NORMAL),
			Instruction("8xy3", "XOR", "%vx, %vy", NORMAL),
			Instruction("8xy4", "ADD_v", "%vx, %vy", NORMAL),
			Instruction("8xy5", "SUB", "%vx, %vy", NORMAL),
			Instruction("8xy6", "SHR", "%vx, %vy", NORMAL),
			Instruction("8xy7", "SUBN", "%vx, %vy", NORMAL),
			Instruction("8xyE", "SHL", "%vx, %vy", NORMAL),
			Instruction("9xy0", "SNE_v", "%vx, %vy", JUMP_SOMETIMES),
			Instruction("Annn", "LD_addr", "I, %addr", NORMAL),
			Instruction("Bnnn", "JP_addr", "V0, %addr", JUMP_ALWAYS),
			Instruction("Cxnn", "RND", "%vx, %byte", NORMAL),
			Instruction("Dxyn", "DRW", "%vx, %vy, %nibble", NORMAL),
			Instruction("Ex9E", "SKP", "%vx", JUMP_SOMETIMES),
			Instruction("ExA1", "SKNP", "%vx", JUMP_SOMETIMES),
			Instruction("Fx07", "LD_vx_dt", "%vx, DT", NORMAL),
			Instruction("Fx0A", "LD_vx_k", "%vx, K", NORMAL),
			Instruction("Fx15", "LD_dt_vx", "DT, %vx", NORMAL),
			Instruction("Fx18", "LD_st_vx", "ST, %vx", NORMAL),
			Instruction("Fx1E", "ADD_i_vx", "I, %vx", NORMAL),
			Instruction("Fx29", "LD_f_vx", "F, %vx", NORMAL),
			Instruction("Fx33", "LD_b_vx", "B, %vx", NORMAL),
			Instruction("Fx55", "LD_Iptr_vx", "[I], %vx", NORMAL),
			Instruction("Fx65", "LD_vx_Iptr", "%vx, [I]", NORMAL),
		};
		InstructionFlags IInstructionTable.InstructionFlags
		{
			get { return InstructionTable.InstructionFlags; }
		}

		delegate void AddVarDelegate(string Name, ref int n, int NibbleCount);

		static private InstructionInfo Instruction(string Opcode, string Mnemonic, string Format, InstructionType InstructionType)
		{
			ushort Mask = 0xFFFF;
			ushort Value = 0x0000;
			var VarReferenceList = new List<VarReference>();

			AddVarDelegate AddVar = (string Name, ref int n, int NibbleCount) =>
			{
				Mask <<= 4 * NibbleCount;
				Value <<= 4 * NibbleCount;
				n += NibbleCount - 1;
				var Shift = (uint)((Opcode.Length - n - 1) * 4);
				VarReferenceList.Add(new VarReference(Name, Shift, (uint)((1 << (4 * NibbleCount)) - 1)));
			};

			for (int n = 0; n < Opcode.Length; n++)
			{
				switch (Opcode[n])
				{
					case 'x': case 'y': AddVar("" + Opcode[n], ref n, 1); break;
					case 'n':
						switch (Opcode.Substring(n))
						{
							case "nnn": AddVar("nnn", ref n, 3); break;
							case "nn": AddVar("nn", ref n, 2); break;
							case "n": AddVar("n", ref n, 1); break;
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

			return new InstructionInfo(Mnemonic, Format, new MaskDataVars(Mask, Value, VarReferenceList.ToArray()))
			{
				InstructionType = InstructionType,
			};
		}
	}
}
