using CSharpCpu.Chip8.Interpreter;
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
			var SwitchTree = SwitchGenerator.GenerateSwitchNoReturnValue(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Statements(
						ast.Statement(ast.CallStatic((Action<CpuContext>)Chip8InterpreterImplementation.INVALID, ast.Argument<CpuContext>(1))),
						ast.Return()
					);
				}

				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;
				var MethodInfo = typeof(Chip8InterpreterImplementation).GetMethod(Context.DecoderReference.Name);

				if (MethodInfo == null)
				{
					throw (new NotImplementedException(String.Format("Can't find implementation for '{0}'", Context.DecoderReference.Name)));
				}

				return ast.Statements(
					ast.Statement(ast.CallStatic(MethodInfo, InstructionTable.ParseParameters(InstructionInfo, Scope, true))),
					ast.Return()
				);
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Action<SwitchReadWordDelegate, CpuContext>>("ExecuteNext", SwitchTree);

			// SwitchCode
			//return 
		}
	}
}
