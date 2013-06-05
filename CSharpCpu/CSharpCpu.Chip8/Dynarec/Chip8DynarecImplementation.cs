using CSharpCpu.Cpus;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Chip8.Dynarec
{
	public sealed class Chip8DynarecImplementation
	{
		static private AstGenerator ast = AstGenerator.Instance;

		static public AstNodeStm SYS(InstructionInfo InstructionInfo, ushort Address)
		{
			return ast.Statement();
		}
	}
}
