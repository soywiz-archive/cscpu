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
	public delegate AstNode GenerateDynarecCodeDelgate(InstructionInfo InstructionInfo, Scope<string, AstLocal> VariableScope);

	public class InstructionDynarecInfo
	{
		public InstructionInfo InstructionInfo;
		public GenerateDynarecCodeDelgate GenerateDynarecCode;

		public InstructionDynarecInfo(InstructionInfo InstructionInfo, GenerateDynarecCodeDelgate GenerateDynarecCode)
		{
			this.InstructionInfo = InstructionInfo;
			this.GenerateDynarecCode = GenerateDynarecCode;
		}
	}
}
