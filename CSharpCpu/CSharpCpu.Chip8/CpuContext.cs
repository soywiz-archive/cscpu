using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public class CpuContext
	{
		public ushort PC;
		public byte[] V = new byte[16];
		public ushort I;

		public IMemory2 Memory;
		public IDisplay Display;
		public ISyscall Syscall;
		public Stack<ushort> CallStack = new Stack<ushort>();
	}
}
