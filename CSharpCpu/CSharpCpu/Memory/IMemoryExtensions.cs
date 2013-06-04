using CSharpCpu.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

static public class IMemory1Extensions
{
	static public void WriteBytes(this IMemory1 Memory, uint Address, byte[] Bytes)
	{
		foreach (var Byte in Bytes)
		{
			//Console.Write("{0} ", Byte);
			Memory.Write1(Address++, Byte);
		}
	}

	static public void WriteStream(this IMemory1 Memory, uint Address, Stream Stream)
	{
		var MemoryStream = new MemoryStream();
		Stream.CopyTo(MemoryStream);
		var Data = MemoryStream.ToArray();
		Memory.WriteBytes(Address, Data);
		//Console.ReadKey();
	}

	//static public ushort Read2BigEndian(this IMemory1 Memory, uint Address)
	//{
	//	return (ushort)((Memory.Read1(Address + 0) << 8) | Memory.Read1(Address + 1));
	//}
	//
	//static public void Write2BigEndian(this IMemory1 Memory, uint Address, ushort Value)
	//{
	//	Memory.Write1(Address + 0, (byte)(Value >> 8));
	//	Memory.Write1(Address + 1, (byte)(Value >> 0));
	//}
}
