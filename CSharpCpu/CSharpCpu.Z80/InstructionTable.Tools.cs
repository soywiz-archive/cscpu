using CSharpCpu.Cpus;
using CSharpCpu.Decoder;
using CSharpCpu.Z80;
using CSharpCpu.Z80.Interpreter;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CSharpCpu.Cpus.Z80
{
	public sealed partial class InstructionTable
	{
		static private AstGenerator ast = AstGenerator.Instance;

		public InstructionFlags InstructionFlags
		{
			get { throw new NotImplementedException(); }
		}

		public static Regex MatchArgument = new Regex(@"%\w+", RegexOptions.Compiled);

		static public List<AstNodeExpr> ParseParameters(InstructionInfo InstructionInfo, Scope<string, AstLocal> Scope)
		{
			var Array = new List<AstNodeExpr>();

			MatchArgument.Replace(InstructionInfo.Name, (Match) =>
			{
				var MatchStr = Match.ToString();
				switch (MatchStr)
				{
					case "%nn":
						Array.Add(
							(new AstNodeExprLocal(Scope.Get("%n2")) * 256) |
							new AstNodeExprLocal(Scope.Get("%n1"))
						);
						break;
					default:
						Array.Add(new AstNodeExprLocal(Scope.Get(MatchStr)));
						break;
				}
				return "";
			});

			return Array;
		}

		static private AstNodeExprLValue GetCpuContext()
		{
			return ast.Argument<CpuContext>(0, "CpuContext");
		}

		static private AstNodeExprLValue GetRegister(string Name)
		{
			return ast.FieldAccess(GetCpuContext(), Name);
		}

		static private AstNodeExpr ReadMemory1(AstNodeExpr Address)
		{
			return ast.CallInstance(GetCpuContext(), (Func<ushort, byte>)CpuContext._NullInstance.ReadMemory1, Address);
		}

		static private AstNodeExpr ParseRightAddress(string RightRegister, Scope<string, AstLocal> Scope)
		{
			switch (RightRegister)
			{
				case "(HL)": return GetRegister("HL");
				case "(IX+%d)": return GetRegister("IX") + ast.Local(Scope.Get("%d"));
				case "(IY+%d)": return GetRegister("IY") + ast.Local(Scope.Get("%d"));
				default: throw(new NotImplementedException("Invalid reference instruction"));
			}
		}

		static private AstNodeExpr ParseRightRegister(string RightRegister, Scope<string, AstLocal> Scope)
		{
			switch (RightRegister)
			{
				case "(HL)":  case "(IX+%d)": case "(IY+%d)": return ReadMemory1(ParseRightAddress(RightRegister, Scope));
				case "%n": return ast.Local(Scope.Get("%n"));
				default: return GetRegister(RightRegister);
			}
		}

		static private AstNodeStm Process(string Mnemonic, Scope<string, AstLocal> Scope)
		{
			Mnemonic = Mnemonic.Trim();
			Match Match;
			if ((Match = (new Regex(@"^(ADC|SBC|ADD|SUB) (A|HL|IX|IY),(SP|BC|DE|HL|IX|IY|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|\(HL\)|(\((IX|IY)\+%d\))|%n)$")).Match(Mnemonic)).Success)
			{
				var Opcode = Match.Groups[1].Value;
				var LeftRegister = Match.Groups[2].Value;
				var RightRegister = Match.Groups[3].Value;
				bool withCarry = (new[] { "ADC", "SBC" }).Contains(Opcode);
				bool isSub = (new[] { "SUB", "SBC" }).Contains(Opcode);

				MethodInfo doArithmeticMethod;
				if (LeftRegister == "A")
				{
					doArithmeticMethod = ((Func<CpuContext, byte, byte, bool, bool, byte>)Z80InterpreterImplementation.doArithmeticByte).Method;
				}
				else
				{
					doArithmeticMethod = ((Func<CpuContext, ushort, ushort, bool, bool, ushort>)Z80InterpreterImplementation.doArithmeticWord).Method;
				}

				return ast.Assign(
					GetRegister(LeftRegister),
					ast.CallStatic(doArithmeticMethod, GetCpuContext(), GetRegister(LeftRegister), ParseRightRegister(RightRegister, Scope), withCarry, isSub)
				);
			}
			if ((Match = (new Regex(@"^(AND|XOR|OR) (\(HL\)|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|%n|(\((IX|IY)\+%d\)))$")).Match(Mnemonic)).Success)
			{
				var Opcode = Match.Groups[1].Value;
				var RightRegister = Match.Groups[2].Value;
				//Console.WriteLine("do" + Opcode);
				return ast.Statement(
					ast.CallStatic(typeof(Z80InterpreterImplementation).GetMethod("do" + Opcode), GetCpuContext(), ParseRightRegister(RightRegister, Scope))
				);
			}
			if ((Match = (new Regex(@"^(BIT|SET|RES) ([0-7]),(\(HL\)|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|%n|(\((IX|IY)\+%d\)))$")).Match(Mnemonic)).Success)
			{
				var Opcode = Match.Groups[1].Value;
				var Bit = int.Parse(Match.Groups[2].Value);
				var RightRegister = Match.Groups[3].Value;
				Console.WriteLine("do" + Opcode + ":" + Bit + ":" + RightRegister);
			}
			return null;
		}

		static private InstructionInfo Instruction(string OpCode, string Mnemonic, int Cycles = 0)
		{
			var Scope = new Scope<string,AstLocal>();
			Scope.Set("%d", AstLocal.Create<byte>("VAR_D"));
			Scope.Set("%n", AstLocal.Create<byte>("VAR_N"));
			Console.WriteLine(GeneratorCSharp.GenerateString(Process(Mnemonic, Scope)));
			var MaskDataVarsList = new List<MaskDataVars>();
			foreach (var Part in OpCode.Split(' '))
			{
				if (Part.Length == 0) continue;

				switch (Part)
				{
					case "%n":
					case "%d":
					case "%e":
						MaskDataVarsList.Add(new MaskDataVars(0x00, 0x00, new VarReference(Part, 0, 0xFF)));
						break;
					case "%nn":
						MaskDataVarsList.Add(new MaskDataVars(0x00, 0x00, new VarReference("%n1", 0, 0xFF)));
						MaskDataVarsList.Add(new MaskDataVars(0x00, 0x00, new VarReference("%n2", 0, 0xFF)));
						break;
					default:
						try
						{
							if ((Part.Length % 2) != 0) throw (new Exception());
							for (int n = 0; n < Part.Length; n += 2)
							{
								MaskDataVarsList.Add(new MaskDataVars(0xFF, Convert.ToUInt32(Part.Substring(n, 2), 16)));
							}
						}
						catch (Exception)
						{
							throw (new Exception("Can't parse '" + Part + "'"));
						}
						break;
				}
			}
			return new InstructionInfo(Mnemonic.Trim(), "", MaskDataVarsList);
		}
	}
}
