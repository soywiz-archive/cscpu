using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Decoder
{
	public class VarReference
	{
		public string Name;
		public uint Shift;
		public uint Mask;

		public VarReference(string Name, uint Shift, uint Mask)
		{
			this.Name = Name;
			this.Shift = Shift;
			this.Mask = Mask;
		}
	}
}
