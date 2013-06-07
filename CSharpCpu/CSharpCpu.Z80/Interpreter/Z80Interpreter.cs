using CSharpCpu.Cpus.Interpreter;
using CSharpCpu.Cpus.Z80;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Optimizers;
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
			var SwitchTree = SwitchGenerator.GenerateSwitchNoReturnValue(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Statements(
						ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.INVALID).Method, ast.Argument<CpuContext>(1))),
						ast.Return()
					);
				}

				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;

				//var Parameters = InstructionTable.ParseParameters(InstructionInfo, Scope);
				//Parameters.Insert(0, ast.Argument<CpuContext>(1));
				//
				//var MethodInfo = typeof(TInterpreterImplementation).GetMethod(Context.DecoderReference.Name);
				//
				//if (MethodInfo == null)
				//{
				//	throw (new NotImplementedException(String.Format("Can't find implementation for '{0}'", Context.DecoderReference.Name)));
				//}

				var Statement = InstructionTable.Process(InstructionInfo, Scope);
				if (Statement == null)
				{
					Statement = ast.Statement(ast.CallStatic(((Action<CpuContext, string, string>)Z80InterpreterImplementation.UNIMPLEMENTED).Method, ast.Argument<CpuContext>(1), InstructionInfo.Name, InstructionInfo.Format));
				}
				return ast.Statements(Statement, ast.Return());
			});
			SwitchTree = (AstNodeStm)(new AstOptimizer().Optimize(SwitchTree));

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Action<SwitchReadWordDelegate, CpuContext>>("ExecuteNext", SwitchTree);
			/*
			return CpuInterpreter.CreateExecuteStep<Z80InterpreterImplementation, CpuContext>(
				InstructionTable.Instructions,
				InstructionTable.ParseParameters
			);
			*/
		}
	}
}
