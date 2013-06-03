using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public interface IDisplay
	{
		void Draw(ref ushort I, ref byte VF, IMemory2 Memory, byte X, byte Y, byte N);
		void Clear();
	}
}
