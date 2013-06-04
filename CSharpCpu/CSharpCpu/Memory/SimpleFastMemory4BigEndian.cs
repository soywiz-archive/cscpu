using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	unsafe public class SimpleFastMemory4BigEndian : IMemory4, IDisposable
	{
		private uint Mask;
		private byte* Data;

		public SimpleFastMemory4BigEndian(int ShiftSize)
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
			//Console.WriteLine("Address: {0:X8}, {1:X8}", Address, Mask);
			return (Data + (Address & Mask));
		}

		public byte Read1(uint Address) { return *(byte*)GetPointer(Address); }
		public ushort Read2(uint Address) { return Swap2(* (ushort*)GetPointer(Address)); }
		public uint Read4(uint Address) { return Swap4(*(uint*)GetPointer(Address)); }

		public void Write1(uint Address, byte Value) { *(byte*)GetPointer(Address) = Value; }
		public void Write2(uint Address, ushort Value) { *(ushort*)GetPointer(Address) = Swap2(Value); }
		public void Write4(uint Address, uint Value) { *(uint*)GetPointer(Address) = Swap4(Value); }

		static private ushort Swap2(ushort Value) { return (ushort)((Value >> 8) | (Value << 8)); }
		static private uint Swap4(uint Value) { return (uint)((Swap2((ushort)(Value >> 0)) << 16) | (Swap2((ushort)(Value >> 16)) << 0)); }
	}
}
