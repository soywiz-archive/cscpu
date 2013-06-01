using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus
{
	public interface IInstructionTable
	{
		InstructionFlags InstructionFlags { get; }
	}
}
