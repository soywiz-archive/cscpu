using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;
using System.Collections.Generic;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using SafeILGenerator.Ast;
using System.Text.RegularExpressions;
using CSharpCpu.Cpus;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class UnitTest3
	{
		static private AstGenerator ast = AstGenerator.Instance;
		static Regex MatchArgument = new Regex(@"%\w+", RegexOptions.Compiled);

		[TestMethod]
		public void TestMethod1()
		{
			var Instructions = CSharpCpu.Cpus.Chip8.InstructionTable.Instructions;

			var SwitchTree = SwitchGenerator.GenerateSwitchReturnValue<string, InstructionInfo>(Instructions, (Context) =>
			{
				if (Context.DecoderReference == null) return ast.Return("-");
				return ast.Return(Context.DecoderReference.Name);
			});


			Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(SwitchTree));
		}
	}
}
