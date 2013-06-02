using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Decoder
{
	public class MaskDataVars
	{
		public uint Mask;
		public uint Data;
		public VarReference[] Vars;

		public MaskDataVars(uint Mask, uint Data, params VarReference[] Vars)
		{
			this.Mask = Mask;
			this.Data = Data;
			this.Vars = Vars;
		}

		public override string ToString()
		{
			return String.Format(
				"MaskDataVars({0:X8}, {1:X8}, [{2}])",
				this.Mask,
				this.Data,
				String.Join(", ", this.Vars.Select(Var => Var.ToString()))
			);
		}
	}
}
