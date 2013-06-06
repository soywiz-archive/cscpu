using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80
{
	public class CpuContext
	{
		static public readonly CpuContext _NullInstance = new CpuContext();

		public ushort AF, BC, DE, HL, IX, IY, SP;
		public Z80Flags F;
		public byte A, C, B, E, D, L, H, IXl, IXh, IYl, IYh;

		public ushort PC;
		public byte R;
		public byte I;
		public byte IFF1;
		public byte IFF2;
		public byte IM;

		public bool halted;
		public uint cycles;

		public bool nmi_req;
		public bool int_req;
		public bool defer_int;
		public bool exec_int_vector;

		public void SETFLAG(Z80Flags Flag)
		{
			this.F |= Flag;
		}

		public void RESFLAG(Z80Flags Flag)
		{
			this.F &= ~Flag;
		}

		public void VALFLAG(Z80Flags Flag, bool Set)
		{
			if (Set) SETFLAG(Flag); else RESFLAG(Flag);
		}

		public bool GETFLAG(Z80Flags Flag)
		{
			return ((this.F & Flag) == Flag);
		}

		public byte ReadMemory1(ushort Address)
		{
			throw(new NotImplementedException("ReadMemory1 not implemented"));
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
