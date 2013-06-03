using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	unsafe public class SimpleFastMemory4 : IMemory4, IDisposable
	{
		private uint Mask;
		private byte* Data;

		public SimpleFastMemory4(int ShiftSize)
		{
			this.Data = (byte*)(Marshal.AllocHGlobal(1 << ShiftSize + 8).ToPointer());
			this.Mask = (uint)((1 << ShiftSize) - 1);
		}

		public void Dispose()
		{
			if (this.Data != null)
			{
				Marshal.FreeHGlobal(new IntPtr(this.Data));
				this.Data = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte* GetPointer(uint Address)
		{
			return (Data + (Address & Mask));
		}

		public byte Read1(uint Address) { return *(byte*)GetPointer(Address); }
		public ushort Read2(uint Address) { return *(ushort*)GetPointer(Address); }
		public uint Read4(uint Address) { return *(uint*)GetPointer(Address); }

		public void Write1(uint Address, byte Value) { *(byte*)GetPointer(Address) = Value; }
		public void Write2(uint Address, ushort Value) { *(ushort*)GetPointer(Address) = Value; }
		public void Write4(uint Address, uint Value) { *(uint*)GetPointer(Address) = Value; }
	}
}
