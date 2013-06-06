using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpCpu.Decoder;
using SafeILGenerator.Ast.Generators;
using System.Collections.Generic;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Utils;
using SafeILGenerator.Ast;
using System.Text.RegularExpressions;
using CSharpCpu.Z80.Disassembler;
using CSharpCpu.Memory;

namespace CSharpCpu.Tests
{
	[TestClass]
	public class Z80Test
	{
		class Memory : IMemory1
		{
			public byte[] data = new byte[0x1000];
			public byte Read1(uint Address) { return data[Address]; }
			public void Write1(uint Address, byte Value) { data[Address] = Value; }
		}

		[TestMethod]
		public void TestDisassembler()
		{
			var Memory = new Memory();
			Memory.WriteBytes(0, new byte[] { 0xAF, 0x21, 0xFF, 0xDF, 0x0E, 0x10, 0x06, 0x00, 0x32, 0x05, 0x20, 0xFC, 0x0D, 0x20, 0xF9, 0x3E });
			var Z80Disassembler = new Z80Disassembler(Memory);
			Z80Disassembler.Address = 0;
			
			for (int n = 0; n < 7; n++)
			{
				Console.WriteLine("{0:X4}: {1}", Z80Disassembler.Address, Z80Disassembler.DecodeNext());
			}
		}

		[TestMethod]
		public void TestInterpreter()
		{
		}
	}
}
