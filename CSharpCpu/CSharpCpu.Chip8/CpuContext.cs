using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpCpu.Chip8;
using CSharpCpu.Chip8.Dynarec;

namespace CSharpCpu.Cpus.Chip8
{
	unsafe public class CpuContext
	{
		static public CpuContext _NullInstance = new CpuContext();

		public ushort PC;
		public byte V0, V1, V2, V3, V4, V5, V6, V7, V8, V9, V10, V11, V12, V13, V14, V15;

		unsafe public class VList
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
		}

		public VList V;

		//public byte[] V = new byte[16];
		public ushort I;
		public readonly Random Random = new Random();

		public readonly IMemory2 Memory;
		public readonly IDisplay Display;
		public readonly ISyscall Syscall;
		public readonly IController Controller;
		public Timer DelayTimer = new Timer("Delay");
		public Timer SoundTimer = new Timer("Sound");
		public Stack<ushort> CallStack = new Stack<ushort>();

		private CpuContext()
		{
		}

		Dictionary<uint, Action<CpuContext>> DynarecCache = new Dictionary<uint, Action<CpuContext>>();

		public Action<CpuContext> GetDelegateForAddress(uint Address)
		{
			Action<CpuContext> Func;
			Console.WriteLine("{0:X8}: GetDelegateForAddress", Address);
			if (!DynarecCache.TryGetValue(Address, out Func))
			{
				DynarecCache[Address] = Func = Chip8Dynarec.CreateDynarecFunction(Memory, PC);
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
