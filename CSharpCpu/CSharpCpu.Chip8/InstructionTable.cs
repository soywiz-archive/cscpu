using CSharpCpu.Decoder;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Chip8
{
	public class InstructionTable : IInstructionTable 
	{
		static InstructionFlags InstructionFlags = new InstructionFlags()
		{
			DecodeSize = 2,
			FixedSize = true,
		};

		public static InstructionInfo[] Instructions = new[]
		{
			Instruction("00E0", "CLS", ""),
			Instruction("00EE", "RET", ""),
			//Instruction("0nnn", "SYS", "%addr"),
			Instruction("1nnn", "JP", "%addr"),
			Instruction("2nnn", "CALL", "%addr"),
			Instruction("3xnn", "SE_n", "%vx, %byte"),
			Instruction("4xnn", "SNE_n", "%vx, %byte"),
			Instruction("5xy0", "SE_v", "%vx, %vy"),
			Instruction("6xnn", "LD_n", "%vx, %byte"),
			Instruction("7xnn", "ADD_n", "%vx, %byte"),
			Instruction("8xy0", "LD_v", "%vx, %vy"),
			Instruction("8xy1", "OR", "%vx, %vy"),
			Instruction("8xy2", "AND", "%vx, %vy"),
			Instruction("8xy3", "XOR", "%vx, %vy"),
			Instruction("8xy4", "ADD_v", "%vx, %vy"),
			Instruction("8xy5", "SUB", "%vx, %vy"),
			Instruction("8xy6", "SHR", "%vx, %vy"),
			Instruction("8xy7", "SUBN", "%vx, %vy"),
			Instruction("8xyE", "SHL", "%vx, %vy"),
			Instruction("9xy0", "SNE_v", "%vx, %vy"),
			Instruction("Annn", "LD_addr", "I, %addr"),
			Instruction("Bnnn", "JP_addr", "V0, %addr"),
			Instruction("Cxnn", "RND", "%vx, %byte"),
			Instruction("Dxyn", "DRW", "%vx, %vy, %nibble"),
			Instruction("Ex9E", "SKP", "%vx"),
			Instruction("ExA1", "SKNP", "%vx"),
			Instruction("Fx07", "LD_vx_dt", "%vx, DT"),
			Instruction("Fx0A", "LD_vx_k", "%vx, K"),
			Instruction("Fx15", "LD_dt_vx", "DT, %vx"),
			Instruction("Fx18", "LD_st_vx", "ST, %vx"),
			Instruction("Fx1E", "ADD_i_vx", "I, %vx"),
			Instruction("Fx29", "LD_f_vx", "F, %vx"),
			Instruction("Fx33", "LD_b_vx", "B, %vx"),
			Instruction("Fx55", "LD_Iptr_vx", "[I], %vx"),
			Instruction("Fx65", "LD_vx_Iptr", "%vx, [I]"),
		};
		InstructionFlags IInstructionTable.InstructionFlags
		{
			get { return InstructionTable.InstructionFlags; }
		}

		delegate void AddVarDelegate(string Name, ref int n, int NibbleCount);

		static private InstructionInfo Instruction(string Opcode, string Mnemonic, string Format)
		{
			ushort Mask = 0xFFFF;
			ushort Value = 0x0000;
			var VarReferenceList = new List<VarReference>();

			AddVarDelegate AddVar = (string Name, ref int n, int NibbleCount) =>
			{
				Mask <<= 4 * NibbleCount;
				Value <<= 4 * NibbleCount;
				n += NibbleCount - 1;
				var Shift = (uint)((Opcode.Length - n - 1) * 4);
				VarReferenceList.Add(new VarReference(Name, Shift, (uint)((1 << (4 * NibbleCount)) - 1)));
			};

			for (int n = 0; n < Opcode.Length; n++)
			{
				switch (Opcode[n])
				{
					case 'x': case 'y': AddVar("" + Opcode[n], ref n, 1); break;
					case 'n':
						switch (Opcode.Substring(n))
						{
							case "nnn": AddVar("nnn", ref n, 3); break;
							case "nn": AddVar("nn", ref n, 2); break;
							case "n": AddVar("n", ref n, 1); break;
							default:
								throw (new Exception("Unexpected"));
						}
						break;
					default:
						Mask <<= 4; Mask |= 0xF;
						Value <<= 4; Value |= Convert.ToUInt16("" + Opcode[n], 16);
						break;
				}
			}

			return new InstructionInfo(Mnemonic, Format, new MaskDataVars(Mask, Value, VarReferenceList.ToArray()));
		}

		static private AstGenerator ast = AstGenerator.Instance;

		public static List<AstNodeExpr> ParseParameters(InstructionInfo InstructionInfo, Scope<string, AstLocal> Scope)
		{
			var Parameters = new List<AstNodeExpr>();
			new Regex(@"%\w+").Replace(InstructionInfo.Format, (Match) =>
			{
				switch (Match.ToString())
				{
					case "%addr": Parameters.Add(ast.Cast<ushort>(ast.Local(Scope.Get("nnn")))); break;
					case "%vx": Parameters.Add(ast.Cast<byte>(ast.Local(Scope.Get("x")))); break;
					case "%vy": Parameters.Add(ast.Cast<byte>(ast.Local(Scope.Get("y")))); break;
					case "%byte": Parameters.Add(ast.Cast<byte>(ast.Local(Scope.Get("nn")))); break;
					case "%nibble": Parameters.Add(ast.Cast<byte>(ast.Local(Scope.Get("n")))); break;
					default: throw (new Exception(Match.ToString()));
				}
				return "";
			});
			return Parameters;
		}
	}
}
