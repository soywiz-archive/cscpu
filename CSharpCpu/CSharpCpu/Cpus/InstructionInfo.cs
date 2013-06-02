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
		public uint[] Mask;
		public uint[] Data;

		public InstructionInfo(string Name, IEnumerable<uint> Mask, IEnumerable<uint> Data)
		{
			this.Name = Name;
			this.Data = Data.ToArray();
			this.Mask = Mask.ToArray();
		}

		uint[] IDecoderReference.Mask { get { return Mask; } }
		uint[] IDecoderReference.Data { get { return Data; } }

		public override string ToString()
		{
			return String.Format(
				"InstructionInfo('{0}', [{1}], [{2}])",
				Name,
				String.Join(",", Mask.Select(Item => String.Format("0x{0:X8}", Item))),
				String.Join(",", Data.Select(Item => String.Format("0x{0:X8}", Item)))
			);
		}
	}
}
