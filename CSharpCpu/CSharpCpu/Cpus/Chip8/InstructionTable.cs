using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public class InstructionTable : IInstructionTable 
	{
		static private InstructionInfo Create(string Name, string Def)
		{
			return null;
		}

		static InstructionFlags InstructionFlags = new InstructionFlags()
		{
			DecodeSize = 2,
			FixedSize = true,
		};

		static InstructionInfo[] Instructions = new[]
		{
			Create("CALL_RCA", "0:NNN"),
			Create("JUMP", "1:NNN"),
			Create("CALL", "2:NNN"),
			Create("SKIP_IF_EQUALS", "3:X:NN"),
			Create("SKIP_IF_NOT_EQUALS", "3:Y:NN"),
			Create("SKIP_IF_EQUALS_VAR", "5:X:Y:N"),
			Create("SET", "6:X:NN"),
			Create("ADD", "7:X:NN"),
			Create("LD", "8:X:Y:0"),
			Create("OR", "8:X:Y:1"),
			Create("AND", "8:X:Y:2"),
			Create("XOR", "8:X:Y:3"),
			Create("ADD", "8:X:Y:4"),
			Create("SUB", "8:X:Y:5"),
			Create("SHR", "8:X:Y:6"),
			Create("SUBN", "8:X:Y:7"),
			Create("SHL", "8:X:Y:E"),
			Create("SKIP_IF_NOT_EQUALS_VAR", "9:X:Y:N"),
			Create("SET_I", "A:NNN"),
			Create("JUMP_ADD", "B:NNN"),
			Create("RAND", "C:-:NN"),
			Create("DRAW", "D:X:Y:N"),
			Create("SKIP_KEY_PRESSED", "E:X:9E"),
			Create("SKIP_KEY_NOT_PRESSED", "E:X:A1"),
			Create("LOAD_TIMER_DELAY", "F:X:07"),
			Create("WAIT_KEY", "F:X:0A"),
			Create("STORE_TIMER_DELAY", "F:X:15"),
			Create("STORE_TIMER_SOUND", "F:X:18"),
			Create("ADD_I", "F:X:1E"),
			Create("SET_I_CHARACTER", "F:X:29"),
			Create("STORE_BDC", "F:X:33"),
			Create("REGS_BACKUP", "F:X:55"),
			Create("REGS_RESTORE", "F:X:65"),
		};

		InstructionFlags IInstructionTable.InstructionFlags
		{
			get { return InstructionTable.InstructionFlags; }
		}
	}
}
