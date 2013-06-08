using CSharpCpu.Decoder;
using CSharpCpu.Memory;
using CSharpCpu.Z80.Disassembler;
using CSharpCpu.Z80.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80
{
	unsafe public sealed class CpuContext
	{
		static public readonly CpuContext _NullInstance = new CpuContext();

		public readonly IMemory2 Memory;
		public readonly IZ80IO IZ80IO;

		//public Z80Flags F;
		public byte F, A, C, B, E, D, L, H, IXl, IXh, IYl, IYh;
		public ushort SP;

		public ushort AF { get { fixed (byte* Ptr = &F  ) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &F  ) *(ushort*)Ptr = value; } }
		public ushort BC { get { fixed (byte* Ptr = &C  ) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &C  ) *(ushort*)Ptr = value; } }
		public ushort DE { get { fixed (byte* Ptr = &E  ) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &E  ) *(ushort*)Ptr = value; } }
		public ushort HL { get { fixed (byte* Ptr = &L  ) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &L  ) *(ushort*)Ptr = value; } }
		public ushort IX { get { fixed (byte* Ptr = &IXl) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &IXl) *(ushort*)Ptr = value; } }
		public ushort IY { get { fixed (byte* Ptr = &IYl) return *(ushort*)Ptr; } set { fixed (byte* Ptr = &IYl) *(ushort*)Ptr = value; } }

		public ushort AF_, BC_, DE_, HL_;

		public ushort PC;
		public byte R;
		public byte I;
		public bool IFF1;
		public bool IFF2;
		public byte IM;

		public bool halted;
		//public uint cycles;

		public uint Tstates;

		public bool nmi_req;
		public bool int_req;
		public bool defer_int;
		public bool exec_int_vector;

		public bool Running;

		private CpuContext()
		{
		}

		public CpuContext(IMemory2 Memory, IZ80IO IZ80IO)
		{
			this.Memory = Memory;
			this.IZ80IO = IZ80IO;
		}

		public void SETFLAG(Z80Flags Flag)
		{
			this.F |= (byte)Flag;
		}

		public void RESFLAG(Z80Flags Flag)
		{
			this.F &= (byte)(~Flag);
		}

		public void VALFLAG(Z80Flags Flag, bool Set)
		{
			if (Set) SETFLAG(Flag); else RESFLAG(Flag);
		}

		public bool GETFLAG(Z80Flags Flag)
		{
			return (((Z80Flags)this.F & Flag) == Flag);
		}

		public void WriteMemory1(ushort Address, byte Value)
		{
			//Console.WriteLine("WriteMemory1({0:X4}, {1:X2})", Address, Value);
			this.Memory.Write1(Address, Value);
		}

		public void WriteMemory2(ushort Address, ushort Value)
		{
			this.Memory.Write2(Address, Value);
		}


		public byte ReadMemory1(ushort Address)
		{
			return this.Memory.Read1(Address);
		}

		public ushort ReadMemory2(ushort Address)
		{
			return this.Memory.Read2(Address);
		}

		public uint ReadInstruction()
		{
			return ReadMemory1(PC++);
		}

		public void Restart()
		{
			F = A = C = B = E = D = L = H = IXl = IXh = IYl = IYh = default(byte);
			SP = 0;

			AF_ = BC_ = DE_ = HL_ = default(ushort);

			PC = 0;
			R = 0;
			I = 0;
			IFF1 = IFF2 = false;
			IM = 0;
			halted = false;
			//cycles = 0;
			Tstates = 0;
			nmi_req = false;
			int_req = false;
			defer_int = false;
			exec_int_vector = false;
		}

		private Lazy<Action<SwitchReadWordDelegate, CpuContext>> ExecuteStepLazy = new Lazy<Action<SwitchReadWordDelegate, CpuContext>>(() => Z80Interpreter.CreateExecuteStep());
		//private Lazy<Z80Disassembler> DissasemblerLazy = new Lazy<Action<SwitchReadWordDelegate, CpuContext>>(() => Z80Interpreter.CreateExecuteStep());

		public void ExecuteTStates(int tstates)
		{
			//var Disassembler = new Z80Disassembler(this.Memory);
			var ExecuteStep = this.ExecuteStepLazy.Value;
			this.Tstates = 0;
			while (this.Tstates < tstates)
			{
				//Console.WriteLine("{0}", Disassembler.DecodeAt(PC));
				var OldTstates = this.Tstates;
				ExecuteStep(ReadInstruction, this);
				if (this.Tstates == OldTstates) throw(new InvalidOperationException("Not updated tstates"));
			}
			this.R = 1;
		}

		public void Run(bool Dynarec)
		{
			Running = true;

			if (Dynarec)
			{
				throw(new NotImplementedException("Z80 Dynarec"));
			}
			else
			{
				var Disassembler = new Z80Disassembler(this.Memory);
				Marshal.PrelinkAll(typeof(Z80InterpreterImplementation));
				var ExecuteStep = this.ExecuteStepLazy.Value;

				int ICount = 0;
				while (Running)
				{
					ushort PC2 = PC;
					//Console.WriteLine("{0:X4}[IF:{1}{2}]: {3}", PC2, IFF1 ? 1 : 0, IFF2 ? 1 : 0, Disassembler.DecodeAt(PC2));
					ExecuteStep(ReadInstruction, this);
					ICount++;
					if (ICount >= 1000)
					{
						ICount = 0;
						Interrupt();
					}
				}
			}
		}

		public void Interrupt()
		{
			if (this.IFF1)
			{
				if (this.halted)
				{
					this.PC++;
					this.halted = false;
				}

				this.Tstates += 7;

				this.R = (byte)((this.R + 1) & 0x7f);
				this.IFF1 = this.IFF2 = false;

				// push PC
				Z80InterpreterImplementation.doPush(this, PC);

				switch (IM)
				{
					case 0: case 1:
						PC = 0x0038;
						break;
					case 2:
						this.PC = ReadMemory2((ushort)(((this.I) << 8) | 0xff));
						break;
					default:
						throw(new InvalidOperationException("Unknown interrupt mode"));
				}
			}
		}

		public byte ioRead(ushort Address)
		{
			return IZ80IO.ioRead(Address);
		}

		public void ioWrite(ushort Address, byte Value)
		{
			IZ80IO.ioWrite(Address, Value);
			//Console.WriteLine("ioWrite: {0:X4}: {1:X2}", Address, Value);
			//throw new NotImplementedException();
		}
	}
}
