using CSharpCpu.Decoder;
using CSharpCpu.Memory;
using CSharpCpu.Z80.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80
{
	public struct Z80Registers
	{
		public ushort AF, BC, DE, HL, IX, IY, SP;
		public Z80Flags F;
		public byte A, C, B, E, D, L, H, IXl, IXh, IYl, IYh;

		public void Restart()
		{
			AF = BC = DE = HL = IX = IY = SP = default(ushort);
			F = default(Z80Flags);
			A = C = B = E = D = L = H = IXl = IXh = IYl = IYh = default(byte);
		}
	}

	public class CpuContext
	{
		static public readonly CpuContext _NullInstance = new CpuContext();

		public readonly IMemory2 Memory;

		public Z80Registers R1 = new Z80Registers();
		public Z80Registers R2 = new Z80Registers();

		public ushort PC;
		public byte R;
		public byte I;
		public bool IFF1;
		public bool IFF2;
		public byte IM;

		public bool halted;
		public uint cycles;

		public bool nmi_req;
		public bool int_req;
		public bool defer_int;
		public bool exec_int_vector;

		public bool Running;

		private CpuContext()
		{
		}

		public CpuContext(IMemory2 Memory)
		{
			this.Memory = Memory;
		}

		public void SETFLAG(Z80Flags Flag)
		{
			this.R1.F |= Flag;
		}

		public void RESFLAG(Z80Flags Flag)
		{
			this.R1.F &= ~Flag;
		}

		public void VALFLAG(Z80Flags Flag, bool Set)
		{
			if (Set) SETFLAG(Flag); else RESFLAG(Flag);
		}

		public bool GETFLAG(Z80Flags Flag)
		{
			return ((this.R1.F & Flag) == Flag);
		}

		public void WriteMemory1(ushort Address, byte Value)
		{
			Console.WriteLine("WriteMemory1({0:X4}, {1:X2})", Address, Value);
			this.Memory.Write1(Address, Value);
		}

		public byte ReadMemory1(ushort Address)
		{
			return this.Memory.Read1(Address);
		}

		public uint ReadInstruction()
		{
			return ReadMemory1(PC++);
		}

		public void Restart()
		{
			R1.Restart();
			R2.Restart();
			PC = 0;
			R = 0;
			I = 0;
			IFF1 = IFF2 = false;
			IM = 0;
			halted = false;
			cycles = 0;
			nmi_req = false;
			int_req = false;
			defer_int = false;
			exec_int_vector = false;
		}

		private Action<SwitchReadWordDelegate, CpuContext> ExecuteStep;

		public void Run(bool Dynarec)
		{
			Running = true;

			if (Dynarec)
			{
				throw(new NotImplementedException("Z80 Dynarec"));
			}
			else
			{
				Marshal.PrelinkAll(typeof(Z80InterpreterImplementation));
				this.ExecuteStep = Z80Interpreter.CreateExecuteStep();

				while (Running)
				{
					Console.WriteLine("PC: {0:X4}, A: {1:X2}", PC, R1.A);
					//Console.Out.Flush();
					//Console.ReadKey();
					this.ExecuteStep(ReadInstruction, this);
				}
			}
		}
	}

	[Flags]
	public enum Z80Flags : byte
	{
		/// <summary>
		/// Carry
		/// </summary>
		F_C = 1 << 0,

		/// <summary>
		/// Sub / Add
		/// </summary>
		F_N = 1 << 1,
		
		/// <summary>
		/// Parity / Overflow
		/// </summary>
		F_PV = 1 << 2,

		/// <summary>
		/// Reserved
		/// </summary>
		F_3 = 1 << 3,

		/// <summary>
		/// Half carry
		/// </summary>
		F_H = 1 << 4,

		/// <summary>
		/// Reserved
		/// </summary>
		F_5 = 1 << 5,

		/// <summary>
		/// Zero
		/// </summary>
		F_Z = 1 << 6,

		/// <summary>
		/// Sign
		/// </summary>
		F_S = 1 << 7,
	}
}
