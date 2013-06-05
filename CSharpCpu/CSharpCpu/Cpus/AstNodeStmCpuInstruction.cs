using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus
{
	public class AstNodeStmCpuInstruction : AstNodeStm
	{
		public InstructionInfo InstructionInfo;
		public AstNodeStm AstNodeStm;

		public AstNodeStmCpuInstruction(InstructionInfo InstructionInfo, AstNodeStm AstNodeStm)
		{
			this.InstructionInfo = InstructionInfo;
			this.AstNodeStm = AstNodeStm;
		}

		public override void TransformNodes(TransformNodesDelegate Transformer)
		{
			Transformer.Ref(ref AstNodeStm);
		}
	}
}
