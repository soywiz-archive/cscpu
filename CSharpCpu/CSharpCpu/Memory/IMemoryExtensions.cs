using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static public class IMemory1Extensions
{
	static public void WriteBytes(this IMemory1 Memory, uint Address, byte[] Data)
	{
		foreach (var Item in Data) Memory.Write1(Address++, Item);
	}
}
