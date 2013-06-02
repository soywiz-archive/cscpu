using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	public interface IMemory1
	{
		byte Read1(uint Address);
		void Write1(uint Address, byte Value);
	}
}
