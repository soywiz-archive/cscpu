using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Chip8
{
	public interface IController
	{
		void WaitPressed();
		bool IsPressed(byte Key);
		byte? GetPressMask();
	}
}
