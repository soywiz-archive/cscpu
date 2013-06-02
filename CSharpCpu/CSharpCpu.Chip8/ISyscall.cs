using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public interface ISyscall
	{
		void Call(CpuContext CpuContext, ushort Address);
	}
}
