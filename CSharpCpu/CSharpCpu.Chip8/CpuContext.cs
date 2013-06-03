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
		public Random Random = new Random();

		public IMemory2 Memory;
		public IDisplay Display;
		public ISyscall Syscall;
		public IController Controller;
		public Timer DelayTimer = new Timer("Delay");
		public Timer SoundTimer = new Timer("Sound");
		public Stack<ushort> CallStack = new Stack<ushort>();
		//public Func<uint> ReadInstruction;
		public void WriteInstruction(ushort Instruction)
		{
			Memory.Write2(PC, Instruction);
			PC += 2;
		}
		public uint ReadInstruction()
		{
			try
			{
				return Memory.Read2(PC);
			}
			finally
			{
				PC += 2;
			}
		}

		public void Update()
		{
			DelayTimer.Update();
			SoundTimer.Update();
		}
	}
}
