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
using CSharpCpu.Decoder;
using CSharpCpu.Cpus.Chip8.Interpreter;

namespace CSharpCpu.Cpus.Chip8
{
	sealed unsafe public class CpuContext
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

			public byte this[int Index]
			{
				get
				{
					fixed (byte* V0Ptr = &CpuContext.V0) return V0Ptr[Index];
				}
				set
				{
					fixed (byte* V0Ptr = &CpuContext.V0) V0Ptr[Index] = value;
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
		public Random Random = new Random();

		public readonly IMemory2 Memory;
		public readonly IDisplay Display;
		public readonly ISyscall Syscall;
		public readonly IController Controller;
		public Chip8Timer DelayTimer = new Chip8Timer("Delay");
		public Chip8Timer SoundTimer = new Chip8Timer("Sound");
		public Stack<ushort> CallStack = new Stack<ushort>();
		public uint InstructionCount;
		public bool Running = false;

		private CpuContext()
		{
		}

		//Dictionary<uint, Action<CpuContext>> DynarecCache = new Dictionary<uint, Action<CpuContext>>();
		Action<CpuContext>[] DynarecCache = new Action<CpuContext>[4096];

		int SubCount2 = 0;

		public void DynarecTick()
		{
			if (!Running)
			{
				throw(new ThreadInterruptedException());
			}

			if (InstructionCount >= 1000)
			{
				//Console.WriteLine("", InstructionCount);
				InstructionCount = 0;
				this.Update();
				if (SubCount2++ % 20 == 0)
				{
					//Console.WriteLine("InstructionCount: {0}: {1}", InstructionCount, String.Join(",", V));
					Display.Update();
				}
				Thread.Sleep(1);
			}
		}

		public Action<CpuContext> GetDelegateForAddress(uint Address)
		{
			Action<CpuContext> Func = DynarecCache[Address];
			//Console.WriteLine("{0:X8}: GetDelegateForAddress", Address);
			if (Func == null)
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

		public void Restart()
		{
			//for (int n = 0; n < 16; n++)
			//{
			//	fixed (byte* V0Ptr = &V0)
			//	fixed (byte* V1Ptr = &V1)
			//	fixed (byte* V2Ptr = &V2)
			//	{
			//		Console.WriteLine("{0}", new IntPtr(V0Ptr));
			//		Console.WriteLine("{0}", new IntPtr(V1Ptr));
			//		Console.WriteLine("{0}", new IntPtr(V2Ptr));
			//	}
			//}

			PC = 0x200;
			for (int n = 0; n < 16; n++) V[n] = 0;
			I = 0;
			Random = new Random();
			//for (int n = 0; n < (1 << 12); n++) Memory.Write1((ushort)n, 0);
			Display.Clear();
			Display.Update();
			DelayTimer = new Chip8Timer("Delay");
			SoundTimer = new Chip8Timer("Sound");
			CallStack = new Stack<ushort>();
			InstructionCount = 0;
			SubCount2 = 0;
			DynarecCache = new Action<CpuContext>[4096];
			Running = true;
		}

		private void _RunDynarec()
		{
			while (true)
			{
				//Console.WriteLine("{0:X8}", Context.PC);
				GetDelegateForAddress(PC)(this);
			}
		}

		static private Lazy<Action<SwitchReadWordDelegate, CpuContext>> LazyInterpretStep = new Lazy<Action<SwitchReadWordDelegate, CpuContext>>(() => Chip8Interpreter.CreateExecuteStep());

		private void _RunInterpreter()
		{
			var InterpretStep = LazyInterpretStep.Value;

			while (true)
			{
				InstructionCount++;
				DynarecTick();
				InterpretStep(ReadInstruction, this);
			}
		}

		public void Run(bool Dynarec)
		{
			try
			{
				Console.WriteLine("ThreadRun...");
				Running = true;
				PC = 0x200;

				if (Dynarec)
				{
					_RunDynarec();
				}
				else
				{
					_RunInterpreter();
				}
			}
			catch (ThreadInterruptedException)
			{
				Console.WriteLine("ThreadInterruptedException");
			}
		}
	}
}
