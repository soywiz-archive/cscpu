using CSharpCpu.Cpus.Interpreter;
using CSharpCpu.Cpus.Z80;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80.Interpreter
{
	public class Z80Interpreter
	{
		static private AstGenerator ast = AstGenerator.Instance;

		static public Action<SwitchReadWordDelegate, CpuContext> CreateExecuteStep()
		{
			return CpuInterpreter.CreateExecuteStep<Z80InterpreterImplementation, CpuContext>(
				InstructionTable.Instructions,
				InstructionTable.ParseParameters
			);
		}
	}
}
