using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Dynarec
{
	public class DynarecResult
	{
		public AstNodeStm AstNodeStm;
		public static implicit operator DynarecResult(AstNodeStm AstNodeStm)
		{
			return new DynarecResult()
			{
				AstNodeStm = AstNodeStm,
			};
		}
	}

	public class DynarecContext
	{
		protected AstGenerator ast = AstGenerator.Instance;
		public Dictionary<uint, AstLabel> PCToLabel = new Dictionary<uint, AstLabel>();
		public uint CurrentPC;
		public uint EndPC;
	}
}
