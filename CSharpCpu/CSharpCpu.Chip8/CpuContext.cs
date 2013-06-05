using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCpu.Chip8;
using CSharpCpu.Chip8.Dynarec;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;

namespace CSharpCpu.Cpus.Chip8
{
	unsafe public class CpuContext
	{
		static public CpuContext _NullInstance = new CpuContext();

		public ushort PC;
		public byte V0, V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12, V13, V14, V15;

		unsafe public class VList : IEnumerable<byte>
		{
			public CpuContext CpuContext;

			public VList(CpuContext CpuContext)
			{
				this.CpuContext = CpuContext;
			}

			public byte this[int i]
			{
				get
				{
					fixed (byte* V0Ptr = &CpuContext.V0) return V0Ptr[i];
				}
				set
				{
					fixed (byte* V0Ptr = &CpuContext.V0) V0Ptr[i] = value;
				}
			}

			IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
			{
				var Items = new byte[16];
				fixed (byte* V0Ptr = &CpuContext.V0) Marshal.Copy(new IntPtr(V0Ptr), Items, 0, 16);
				return Items.AsEnumerable<byte>().GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				var Items = new byte[16];
				fixed (byte* V0Ptr = &CpuContext.V0) Marshal.Copy(new IntPtr(V0Ptr), Items, 0, 16);
				return Items.GetEnumerator();
			}
		}

		public VList V;

		//public byte[] V = new byte[16];
		public ushort I;
		public readonly Random Random = new Random();

		public readonly IMemory2 Memory;
		public readonly IDisplay Display;
		public readonly ISyscall Syscall;
		public readonly IController Controller;
		public Chip8Timer DelayTimer = new Chip8Timer("Delay");
		public Chip8Timer SoundTimer = new Chip8Timer("Sound");
		public Stack<ushort> CallStack = new Stack<ushort>();
		public uint InstructionCount;
		public bool Running = true;

		private CpuContext()
		{
		}

		Dictionary<uint, Action<CpuContext>> DynarecCache = new Dictionary<uint, Action<CpuContext>>();

		int SubCount2 = 0;

		public void DynarecTick()
		{
			if (InstructionCount >= 1000)
			{
				//Console.WriteLine("", InstructionCount);
				InstructionCount = 0;
				this.Update();
				if (SubCount2++ % 20 == 0)
				{
					Console.WriteLine("InstructionCount: {0}: {1}", InstructionCount, String.Join(",", V));
					Display.Update();
				}
				Thread.Sleep(1);
			}
		}

		public Action<CpuContext> GetDelegateForAddress(uint Address)
		{
			Action<CpuContext> Func;
			//Console.WriteLine("{0:X8}: GetDelegateForAddress", Address);
			if (!DynarecCache.TryGetValue(Address, out Func))
			{
				DynarecCache[Address] = Func = Chip8Dynarec.CreateDynarecFunction(Memory, Address);
			}
			return Func;
		}

		public CpuContext(IMemory2 Memory, IDisplay Display, ISyscall Syscall, IController Controller)
		{
			this.Memory = Memory;
			this.Display = Display;
			this.Syscall = Syscall;
			this.Controller = Controller;
			this.V = new VList(this);
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
