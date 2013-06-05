using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Dynarec
{
	public enum BranchType
	{
		NoFollowAnalyzedAddresses = 0,
		FollowAnalyzedAddresses = 1,
	}

	public enum AnalyzeType
	{
		StopAnalyzingCurrentBranch = 0,
		ContinueAnalyzingCurrentBranch = 1,
	}

	public class BranchResult
	{
		public BranchType BranchType;
		public AnalyzeType AnalyzeType;
		public uint[] PossibleJumpList;

		public BranchResult(AnalyzeType AnalyzeType, BranchType BranchType, params uint[] PossibleJumpList)
		{
			this.BranchType = BranchType;
			this.AnalyzeType = AnalyzeType;
			this.PossibleJumpList = PossibleJumpList;
		}

		public bool ContinueAnalyzing { get { return AnalyzeType == AnalyzeType.ContinueAnalyzingCurrentBranch; } }
		public bool FollowJumps { get { return BranchType == BranchType.FollowAnalyzedAddresses; } }
	}

	public class BranchContext
	{
		public uint InstructionAddress;
		public uint EndInstructionAddress;

		public BranchContext(uint InstructionAddress, uint EndInstructionAddress)
		{
			this.InstructionAddress = InstructionAddress;
			this.EndInstructionAddress = EndInstructionAddress;
		}
	}
}
