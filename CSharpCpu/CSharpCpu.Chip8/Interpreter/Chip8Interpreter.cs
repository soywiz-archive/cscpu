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

		static public Action<Func<uint>, CpuContext> CreateExecuteStep()
		{
			var SwitchTree = SwitchGenerator.GenerateSwitchNoReturnValue(InstructionTable.Instructions, (Context) =>
			{
				if (Context.DecoderReference == null)
				{
					return ast.Statements(
						ast.Statement(ast.CallStatic((Action<CpuContext>)Chip8Interpreter.INVALID, ast.Argument<CpuContext>(1))),
						ast.Return()
					);
				}

				var InstructionInfo = Context.DecoderReference;
				var MethodInfo = typeof(Chip8Interpreter).GetMethod(Context.DecoderReference.Name);

				if (MethodInfo == null)
				{
					throw (new NotImplementedException(String.Format("Can't find implementation for '{0}'", Context.DecoderReference.Name)));
				}

				var Parameters = new List<AstNodeExpr>();
				Parameters.Add(ast.Argument<CpuContext>(1));
				new Regex(@"%\w+").Replace(InstructionInfo.Format, (Match) =>
				{
					switch (Match.ToString())
					{
						case "%addr": Parameters.Add(ast.Cast<ushort>(ast.Local(Context.Scope.Get("nnn")))); break;
						case "%vx": Parameters.Add(ast.Cast<byte>(ast.Local(Context.Scope.Get("x")))); break;
						case "%vy": Parameters.Add(ast.Cast<byte>(ast.Local(Context.Scope.Get("y")))); break;
						case "%byte": Parameters.Add(ast.Cast<byte>(ast.Local(Context.Scope.Get("nn")))); break;
						case "%nibble": Parameters.Add(ast.Cast<byte>(ast.Local(Context.Scope.Get("n")))); break;
						default: throw(new Exception(Match.ToString()));
					}
					return "";
				});

				return ast.Statements(
					ast.Statement(ast.CallStatic(MethodInfo, Parameters.ToArray())),
					ast.Return()
				);
			});

			//Console.WriteLine(GeneratorCSharp.GenerateString(SwitchTree));

			return GeneratorIL.GenerateDelegate<GeneratorIL, Action<Func<uint>, CpuContext>>("ExecuteNext", SwitchTree);

			// SwitchCode
			//return 
		}
	}
}
