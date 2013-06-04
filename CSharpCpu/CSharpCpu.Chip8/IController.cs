using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpCpu.Chip8
{
	public interface IController
	{
		bool IsPressed(byte Key);
		byte? GetPressMask();
	}
}
