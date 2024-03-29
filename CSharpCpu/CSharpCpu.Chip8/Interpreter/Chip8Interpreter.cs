﻿using CSharpCpu.Chip8.Interpreter;
using CSharpCpu.Cpus.Interpreter;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8.Interpreter
{
	public sealed partial class Chip8Interpreter
	{
		static private AstGenerator ast = AstGenerator.Instance;

		static public Action<SwitchReadWordDelegate, CpuContext> CreateExecuteStep()
		{
			return CpuInterpreter.CreateExecuteStep<Chip8InterpreterImplementation, CpuContext>(
				InstructionTable.Instructions,
				InstructionTable.ParseParameters
			);
		}
	}
}
