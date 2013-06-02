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
	}
}
