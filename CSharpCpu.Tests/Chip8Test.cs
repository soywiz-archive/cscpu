using CSharpCpu.Chip8;
using CSharpCpu.Cpus.Chip8;
using CSharpCpu.Cpus.Chip8.Interpreter;
using CSharpCpu.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class Chip8Test
	{
		class Syscall : ISyscall
		{
			void ISyscall.Call(CpuContext CpuContext, ushort Address)
			{
				//throw new NotImplementedException();
			}
		}

		class Display : IDisplay
		{
			public void Draw(ref ushort I, ref byte VF, IMemory2 Memory, byte X, byte Y, byte N)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public void Update()
			{
				throw new NotImplementedException();
			}
		}

		class Controller : IController
		{
			public bool IsPressed(byte Key)
			{
				throw new NotImplementedException();
			}

			public byte? GetPressMask()
			{
				throw new NotImplementedException();
			}
		}

		[TestMethod]
		public void TestInterpreter()
		{
			var ExecuteStep = Chip8Interpreter.CreateExecuteStep();
			var CpuContext = new CpuContext(new SimpleFastMemory4(12), new Display(), new Syscall(), new Controller());

			var Instructions = new ushort[] {
				0x70FF,
				0x7102,
				0x7103,
			};

			CpuContext.PC = 0;
			foreach (var Instruction in Instructions)
			{
				CpuContext.WriteInstruction(Instruction);
			}
			CpuContext.PC = 0;
			
			Assert.AreEqual(0, CpuContext.PC);
			Assert.AreEqual(0x00, CpuContext.V[0]);
			Assert.AreEqual(0x00, CpuContext.V[1]);
			{
				for (int n = 0; n < Instructions.Length; n++)
				{
					ExecuteStep(CpuContext.ReadInstruction, CpuContext);
				}
			}
			Assert.AreEqual(Instructions.Length * 2, CpuContext.PC);
			Assert.AreEqual(0xFF, CpuContext.V[0]);
			Assert.AreEqual(0x05, CpuContext.V[1]);
		}
	}
}
