using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Utils
{
	public class LangUtils
	{
		static public void Swap<T>(ref T a, ref T b)
		{
			T tmp = a;
			a = b;
			b = tmp;
		}

		static public T SwapRet<T>(T a, ref T b)
		{
			T tmp = b;
			b = a;
			return tmp;
		}
	}
}
