using CSharpCpu.Decoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Cpus
{
	public class InstructionInfo : IDecoderReference 
	{
		public string Name;
		public MaskDataVars[] MaskDataVarsList;

		public InstructionInfo(string Name, IEnumerable<MaskDataVars> MaskDataVarsList)
		{
			this.Name = Name;
			this.MaskDataVarsList = MaskDataVarsList.ToArray();
		}

		public InstructionInfo(string Name, params MaskDataVars[] MaskDataVarsList)
		{
			this.Name = Name;
			this.MaskDataVarsList = MaskDataVarsList.ToArray();
		}

		MaskDataVars[] IDecoderReference.MaskDataVars { get { return MaskDataVarsList; } }

		public override string ToString()
		{
			return String.Format(
				"InstructionInfo('{0}', [{1}])",
				Name,
				String.Join(",", MaskDataVarsList.Select(Item => Item.ToString()))
			);
		}

	}
}
