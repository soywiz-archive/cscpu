using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static public class IMemory1Extensions
{
	static public void WriteBytes(this IMemory1 Memory, uint Address, byte[] Data)
	{
		foreach (var Item in Data) Memory.Write1(Address++, Item);
	}

	static public void WriteStream(this IMemory1 Memory, uint Address, Stream Stream)
	{
		var MemoryStream = new MemoryStream();
		Stream.CopyTo(MemoryStream);
		var Data = MemoryStream.ToArray();
		Memory.WriteBytes(Address, Data);
	}

}
