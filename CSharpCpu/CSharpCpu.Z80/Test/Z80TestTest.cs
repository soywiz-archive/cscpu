using CSharpCpu.Memory;
using CSharpCpu.Z80.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpCpu.Z80.Test
{
	public class Z80TestTest
	{
		class Memory : IMemory1
		{
			public byte[] data = new byte[0x1000];
			public byte Read1(uint Address) { return data[Address]; }
			public void Write1(uint Address, byte Value) { data[Address] = Value; }
		}

		class Port : IZ80IO
		{
			void IZ80IO.ioWrite(ushort Address, byte Value) { }

			byte IZ80IO.ioRead(ushort Address) { return 0; }
		}

		static public void Test()
		{
			var Step = Z80Interpreter.CreateExecuteStep();
			var Memory = new SimpleFastMemory4(13);
			var Port = new Port();
			var CpuContext = new CSharpCpu.Z80.CpuContext(Memory, Port);

			var TestsZ80Path = @"C:\projects\csmultiemu\tests\z80";
			var TestsInStream = new StreamReader(File.OpenRead(TestsZ80Path + @"\tests.in"));
			var SpaceRegex = new Regex(@"\s+");

			var ExecuteStep = Z80Interpreter.CreateExecuteStep();

			int TestCount = 0;
			while (!TestsInStream.EndOfStream)
			{
				TestCount++;
				string TestName = "";
				while (TestName.Length == 0)
				{
					TestName = TestsInStream.ReadLine().Trim();
				}

				//Console.WriteLine("{0}", TestName);

				string RegistersLine = TestsInStream.ReadLine().Trim();

				var Registers = new Queue<ushort>(SpaceRegex.Split(RegistersLine).Select(Item => Convert.ToUInt16(Item, 16)).ToArray());
				CpuContext.AF = Registers.Dequeue();
				CpuContext.BC = Registers.Dequeue();
				CpuContext.DE = Registers.Dequeue();
				CpuContext.HL = Registers.Dequeue();
				CpuContext.AF_ = Registers.Dequeue();
				CpuContext.BC_ = Registers.Dequeue();
				CpuContext.DE_ = Registers.Dequeue();
				CpuContext.HL_ = Registers.Dequeue();
				CpuContext.IX = Registers.Dequeue();
				CpuContext.IY = Registers.Dequeue();
				CpuContext.SP = Registers.Dequeue();
				CpuContext.PC = Registers.Dequeue();
				var Registers2 = new Queue<string>(SpaceRegex.Split(TestsInStream.ReadLine()));
				CpuContext.I = Convert.ToByte(Registers2.Dequeue(), 16);
				CpuContext.R = Convert.ToByte(Registers2.Dequeue(), 16);
				CpuContext.IFF1 = Convert.ToByte(Registers2.Dequeue()) != 0;
				CpuContext.IFF2 = Convert.ToByte(Registers2.Dequeue()) != 0;
				CpuContext.IM = Convert.ToByte(Registers2.Dequeue(), 16);
				CpuContext.halted = Convert.ToByte(Registers2.Dequeue()) != 0;
				int tstates = Convert.ToUInt16(Registers2.Dequeue(), 16);
				while (!TestsInStream.EndOfStream)
				{
					var Line1 = TestsInStream.ReadLine().Trim();
					if (Line1 == "-1") break;
					var Parts = new Queue<string>(SpaceRegex.Split(Line1));
					var Address = Convert.ToUInt16(Parts.Dequeue(), 16);
					while (Parts.Count > 0)
					{
						var Part = Parts.Dequeue();
						if (Part == "-1") break;
						var Byte = Convert.ToByte(Part, 16);
						CpuContext.Memory.Write1(Address++, Byte);
					}
				}

				CpuContext.ExecuteTStates(tstates);
				Console.WriteLine("{0}", TestName);
				Console.WriteLine(
					"{0:x4} {1:x4} {2:x4} {3:x4} {4:x4} {5:x4} {6:x4} {7:x4} {8:x4} {9:x4} {10:x4} {11:x4}",
					CpuContext.AF,
					CpuContext.BC,
					CpuContext.DE,
					CpuContext.HL,
					CpuContext.AF_,
					CpuContext.BC_,
					CpuContext.DE_,
					CpuContext.HL_,
					CpuContext.IX,
					CpuContext.IY,
					CpuContext.SP,
					CpuContext.PC
				);
				Console.WriteLine(
					"{0:x2} {1:x2} {2} {3} {4} {5} {6}",
					CpuContext.I,
					CpuContext.R,
					CpuContext.IFF1 ? 1 : 0,
					CpuContext.IFF2 ? 1 : 0,
					CpuContext.IM,
					CpuContext.halted ? 1 : 0,
					CpuContext.Tstates
				);
				Console.WriteLine("");

				//if (TestCount >= 60) break;

				//ExecuteStep(CpuContext.ReadInstruction, CpuContext);
				//CpuContext.run
			}
		}
	}
}
