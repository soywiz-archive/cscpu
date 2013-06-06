using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Interpreter
{
	public class CpuInterpreter
	{
		static private AstGenerator ast = AstGenerator.Instance;

		static public Action<SwitchReadWordDelegate, TCpuContext> CreateExecuteStep<TInterpreterImplementation, TCpuContext>(InstructionInfo[] InstructionTable, Func<InstructionInfo, Scope<string, AstLocal>, List<AstNodeExpr>> ParseParameters)
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchNoReturnValue(InstructionTable, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Statements(
						ast.Statement(ast.CallStatic(typeof(TInterpreterImplementation).GetMethod("INVALID"), ast.Argument<TCpuContext>(1))),
						ast.Return()
					);
				}

				var InstructionInfo = Context.DecoderReference;
				var Scope = Context.Scope;

				var Parameters = ParseParameters(InstructionInfo, Scope);
				Parameters.Insert(0, ast.Argument<TCpuContext>(1));

				var MethodInfo = typeof(TInterpreterImplementation).GetMethod(Context.DecoderReference.Name);

				if (MethodInfo == null)
				{
					throw (new NotImplementedException(String.Format("Can't find implementation for '{0}'", Context.DecoderReference.Name)));
				}

				return ast.Statements(
					ast.Statement(ast.CallStatic(MethodInfo, Parameters.ToArray())),
					ast.Return()
				);
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Action<SwitchReadWordDelegate, TCpuContext>>("ExecuteNext", SwitchTree);

			// SwitchCode
			//return 
		}
	}
}
