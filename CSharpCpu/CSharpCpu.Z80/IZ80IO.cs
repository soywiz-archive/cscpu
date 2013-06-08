using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80
{
	public interface IZ80IO
	{
		void ioWrite(ushort Address, byte Value);
		byte ioRead(ushort Address);
	}
}
