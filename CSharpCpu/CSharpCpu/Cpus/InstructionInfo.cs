﻿using CSharpCpu.Decoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Cpus
{
	public class InstructionInfo : IDecoderReference 
	{
		public string Name;
		public string Format;
		public MaskDataVars[] MaskDataVarsList;
		public InstructionType InstructionType;

		public InstructionInfo(string Name, string Format, IEnumerable<MaskDataVars> MaskDataVarsList)
		{
			this.Name = Name;
			this.Format = Format;
			this.MaskDataVarsList = MaskDataVarsList.ToArray();
		}

		public InstructionInfo(string Name, string Format, params MaskDataVars[] MaskDataVarsList)
		{
			this.Name = Name;
			this.Format = Format;
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
