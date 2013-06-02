using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	public interface IMemory4 : IMemory2
	{
		uint Read4(uint Address);
		void Write4(uint Addres, uint Value);
	}
}
