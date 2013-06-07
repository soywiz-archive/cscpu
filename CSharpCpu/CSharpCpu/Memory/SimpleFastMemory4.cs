using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	unsafe public class SimpleFastMemory4 : MemoryPointer, IDisposable
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
		public override byte* GetPointer(uint Address, int Size)
		{
			//Console.WriteLine("Address: {0:X8}, {1:X8}", Address, Mask);
			return (Data + (Address & Mask));
		}
	}
}
