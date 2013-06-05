using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Dynarec
{
	public enum BranchType
	{
		NoJump = 0,
		FixedDestination = 1,
		UnknownDestination = 2,
	}

	public class BranchResult
	{
		public BranchType BranchType;
		public uint[] PossibleJumpList;

		public BranchResult(BranchType BranchType, params uint[] PossibleJumpList)
		{
			this.PossibleJumpList = PossibleJumpList;
		}
	}


	public class BranchContext
	{
		public uint InstructionAddress;
		public uint EndInstructionAddress;
	}
}
