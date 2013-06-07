using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	unsafe public abstract class MemoryPointer : IMemory4
	{
		public abstract byte* GetPointer(uint Address, int Size);

		public byte Read1(uint Address) { return *(byte*)GetPointer(Address, 1); }
		public ushort Read2(uint Address) { return *(ushort*)GetPointer(Address, 2); }
		public uint Read4(uint Address) { return *(uint*)GetPointer(Address, 4); }

		public void Write1(uint Address, byte Value) { *(byte*)GetPointer(Address, 1) = Value; }
		public void Write2(uint Address, ushort Value) { *(ushort*)GetPointer(Address, 2) = Value; }
		public void Write4(uint Address, uint Value) { *(uint*)GetPointer(Address, 4) = Value; }
	}
}
