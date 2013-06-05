using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Dynarec
{
	//public delegate AstNode GenerateDynarecCodeDelgate(InstructionInfo InstructionInfo, Scope<string, AstLocal> VariableScope);

	public class InstructionDynarecInfo
	{
		public InstructionInfo InstructionInfo;
		public uint[] Params;

		public InstructionDynarecInfo(InstructionInfo InstructionInfo, uint[] Params)
		{
			this.InstructionInfo = InstructionInfo;
			this.Params = Params;
			//this.GenerateDynarecCode = GenerateDynarecCode;
		}

		public override string ToString()
		{
			return String.Format(
				"InstructionDynarecInfo({0}, [{1}])",
				InstructionInfo.ToString(),
				String.Join(", ", Params)
			);
		}
	}
}
