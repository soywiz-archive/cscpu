using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Memory
{
	public interface IMemory2 : IMemory1
	{
		ushort Read2(uint Address);
		void Write2(uint Addres, ushort Value);
	}
}
