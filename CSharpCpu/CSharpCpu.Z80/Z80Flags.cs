using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80
{
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
