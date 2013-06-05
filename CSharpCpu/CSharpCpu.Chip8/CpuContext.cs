using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCpu.Chip8;

namespace CSharpCpu.Cpus.Chip8
{
	public class CpuContext
	{
		public ushort PC;
		public byte[] V = new byte[16];
		public ushort I;
		public readonly Random Random = new Random();

		public readonly IMemory2 Memory;
		public readonly IDisplay Display;
		public readonly ISyscall Syscall;
		public readonly IController Controller;
		public Timer DelayTimer = new Timer("Delay");
		public Timer SoundTimer = new Timer("Sound");
		public Stack<ushort> CallStack = new Stack<ushort>();

		public CpuContext(IMemory2 Memory, IDisplay Display, ISyscall Syscall, IController Controller)
		{
			this.Memory = Memory;
			this.Display = Display;
			this.Syscall = Syscall;
			this.Controller = Controller;
		}

		//public SwitchReadWordDelegate ReadInstruction;
		public void WriteInstruction(ushort Instruction)
		{
			Memory.Write2(PC, Instruction);
			PC += 2;
		}
		public uint ReadInstruction()
		{
			var Readed = Memory.Read2(PC);
			PC += 2;
			return Readed;
		}

		public void Update()
		{
			DelayTimer.Update();
			SoundTimer.Update();
			//Display.Update();
		}
	}
}
