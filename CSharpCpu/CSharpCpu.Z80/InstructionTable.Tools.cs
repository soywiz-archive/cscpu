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
			return ast.Argument<CpuContext>(1, "CpuContext");
		}

		//static private AstNodeExprLValue GetR1()
		//{
		//	return ast.FieldAccess(GetCpuContext(), "R1");
		//}

		static private AstNodeExprLValue GetRegister(string Name)
		{
			return ast.FieldPropertyAccess(GetCpuContext(), Name);
		}

		static private AstNodeExpr ReadMemory1(AstNodeExpr Address)
		{
			return ast.CallInstance(GetCpuContext(), (Func<ushort, byte>)CpuContext._NullInstance.ReadMemory1, Address);
		}

		static private AstNodeExpr ReadMemory2(AstNodeExpr Address)
		{
			return ast.CallInstance(GetCpuContext(), (Func<ushort, ushort>)CpuContext._NullInstance.ReadMemory2, Address);
		}

		static private AstNodeStm WriteMemory1(AstNodeExpr Address, AstNodeExpr Value)
		{
			return ast.Statement(ast.CallInstance(GetCpuContext(), (Action<ushort, byte>)CpuContext._NullInstance.WriteMemory1, Address, Value));
		}

		static private AstNodeStm WriteMemory2(AstNodeExpr Address, AstNodeExpr Value)
		{
			return ast.Statement(ast.CallInstance(GetCpuContext(), (Action<ushort, ushort>)CpuContext._NullInstance.WriteMemory2, Address, Value));
		}

		static private AstNodeExpr GetFlag(Z80Flags Flag)
		{
			return ast.Binary(ast.Binary(GetRegister("F"), "&", (byte)Flag), "!=", (byte)0);
		}

		static private AstNodeExpr GetFlag(string Flag)
		{
			switch (Flag)
			{
				case "": return true;
				case "Z": return GetFlag(Z80Flags.F_Z);
				case "NZ": return ast.Unary("!", GetFlag(Z80Flags.F_Z));
				case "C": return GetFlag(Z80Flags.F_C);
				case "NC": return ast.Unary("!", GetFlag(Z80Flags.F_C));
				case "M": return GetFlag(Z80Flags.F_S);
				case "P": return ast.Unary("!", GetFlag(Z80Flags.F_S | Z80Flags.F_Z));
				case "PE": return ast.Unary("!", GetFlag(Z80Flags.F_PV));
				default: return ast.Unary("!", GetFlag(Z80Flags.F_PV));
			}
		}

		static private AstNodeExpr ParseRightAddress(string RightRegister, Scope<string, AstLocal> Scope)
		{
			switch (RightRegister)
			{
				case "(HL)": return GetRegister("HL");
				case "(IX+%d)": return ast.Cast<ushort>(ast.Cast<int>(GetRegister("IX")) + ast.Cast<int>(ast.Local(Scope.Get("%d"))));
				case "(IY+%d)": return ast.Cast<ushort>(ast.Cast<int>(GetRegister("IY")) + ast.Cast<int>(ast.Local(Scope.Get("%d"))));
				//case "IX": return ast.Cast<ushort>(ast.Cast<int>(GetRegister("IX")) + ast.Cast<int>(ast.Local(Scope.Get("%d"))));
				//case "IY": return ast.Cast<ushort>(ast.Cast<int>(GetRegister("IY")) + ast.Cast<int>(ast.Local(Scope.Get("%d"))));
				default: throw (new NotImplementedException("Invalid reference instruction"));
			}
		}

		static private AstNodeExpr ParseRightRegister(string RightRegister, Scope<string, AstLocal> Scope)
		{
			switch (RightRegister)
			{
				case "(HL)":  case "(IX+%d)": case "(IY+%d)": return ReadMemory1(ParseRightAddress(RightRegister, Scope));
				case "%n": return ast.Cast<byte>(ast.Local(Scope.Get("%n")));
				default: return GetRegister(RightRegister);
			}
		}

		static private Dictionary<string, Regex> RegexCache = new Dictionary<string, Regex>();
		static private Regex GetRegex(string Text)
		{
			//return new Regex(Text);
			if (!RegexCache.ContainsKey(Text))
			{
				RegexCache[Text] = new Regex(Text);
			}
			return RegexCache[Text];
		}

		static private AstNodeExpr GetNNWord(Scope<string, AstLocal> Scope)
		{
			return ast.Cast<ushort>(ast.Binary(ast.Local(Scope.Get("%n1")), "|", ast.Binary(ast.Local(Scope.Get("%n2")), "<<", 8)));
		}

		static private AstNodeExpr GetNByte(Scope<string, AstLocal> Scope)
		{
			return ast.Cast<byte>(ast.Local(Scope.Get("%n")));
		}

		static private AstNodeExpr GetDByte(Scope<string, AstLocal> Scope)
		{
			return ast.Cast<sbyte>(ast.Local(Scope.Get("%d")));
		}

		//static private AstNodeExpr Read8(AstNodeExpr Address)
		//{
		//	return ast.CallInstance(GetCpuContext(), (Func<ushort, byte>)CpuContext._NullInstance.ReadMemory1, Address);
		//}

		static public AstNodeStm Process(InstructionInfo InstructionInfo, Scope<string, AstLocal> Scope)
		{
			var Result = _Process(InstructionInfo, Scope);
			if (Result == null) return null;
			return ast.Statements(
				ast.Assign(GetRegister("Tstates"), GetRegister("Tstates") + InstructionInfo.Tstates),
				Result
			);
		}

		static public AstNodeStm _Process(InstructionInfo InstructionInfo, Scope<string, AstLocal> Scope)
		{
			//Mnemonic = Mnemonic.Trim();
			//var Parts = Mnemonic.Split(new [] {' ' }, 2);
			Match Match;

			var Opcode = InstructionInfo.Name;
			var Param = InstructionInfo.Format;

			switch (Opcode)
			{
				case "ADC": case "SBC": case "ADD": case "SUB":
					if ((Match = (GetRegex(@"^(A|HL|IX|IY),(SP|BC|DE|HL|IX|IY|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|\(HL\)|(\((IX|IY)\+%d\))|%n)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						var RightRegister = Match.Groups[2].Value;
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
				break;
				case "CP":
					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl)$")).Match(Param)).Success)
					{
						var Register = Match.Groups[1].Value;
						return ast.Statements(
							ast.Statement(ast.CallStatic(
								(Func<CpuContext, byte, byte, bool, bool, byte>)Z80InterpreterImplementation.doArithmeticByte,
								GetCpuContext(), GetRegister("A"), GetRegister(Register), false, true
							)),
							ast.Statement(ast.CallStatic(
								(Action<CpuContext, byte>)Z80InterpreterImplementation.adjustFlags,
								GetCpuContext(), GetRegister(Register)
							))
						);
					}

					if ((Match = (GetRegex(@"^\(HL\)$")).Match(Param)).Success)
					{
						return ast.Statement(ast.CallStatic(
							(Func<CpuContext, byte>)Z80InterpreterImplementation.doCP_HL,
							GetCpuContext()
						));
					}
					if ((Match = (GetRegex(@"^(%n)$")).Match(Param)).Success)
					{
						return ast.Statements(
							ast.Assign(GetRegister("A"), ast.CallStatic((Func<CpuContext, byte, byte, bool, bool, byte>)Z80InterpreterImplementation.doArithmeticByte, GetCpuContext(), GetRegister("A"), GetNByte(Scope), false, true)),
							ast.Statement(ast.CallStatic((Action<CpuContext, byte>)Z80InterpreterImplementation.adjustFlags, GetCpuContext(), GetNByte(Scope)))
						);
					}
					break;
				// (RLC|RRC|RL|RR) 
				case "RLC": case "RRC": case "RL": case "RR":
					{
						var FuncName = "do" + Opcode;
						if ((Match = (GetRegex(@"^\(HL\)$")).Match(Param)).Success)
						{
							return WriteMemory1(
								GetRegister("HL"),
								ast.CallStatic(typeof(Z80InterpreterImplementation).GetMethod(FuncName), GetCpuContext(), true, ReadMemory1(GetRegister("HL")))
							);
						}
						if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl)$")).Match(Param)).Success)
						{
							var Register = Match.Groups[1].Value;
							return ast.Assign(
								GetRegister(Register),
								ast.CallStatic(typeof(Z80InterpreterImplementation).GetMethod(FuncName), GetCpuContext(), true, GetRegister(Register))
							);
						}
					}
					break;
				case "RLA": case "RRA": case "RLCA": case "RRCA":
					if ((Match = (GetRegex(@"^(RL|RR|RLC|RRC)A$")).Match(Opcode)).Success)
					{
						var FName = "do" + Match.Groups[1].Value;
						var doMethod = typeof(Z80InterpreterImplementation).GetMethod(FName);
						if (doMethod == null) throw (new Exception("Can't find method '" + FName + "'"));
						return ast.Assign(GetRegister("A"), ast.CallStatic(doMethod, GetCpuContext(), false, GetRegister("A")));
					}
					break;
				case "AND": case "XOR": case "OR":
					if ((Match = (GetRegex(@"^(\(HL\)|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|%n|(\((IX|IY)\+%d\)))$")).Match(Param)).Success)
					{
						var RightRegister = Match.Groups[1].Value;
						//Console.WriteLine("do" + Opcode);
						return ast.Statement(
							ast.CallStatic(
								typeof(Z80InterpreterImplementation).GetMethod("do" + Opcode),
								GetCpuContext(), ParseRightRegister(RightRegister, Scope)
							)
						);
					}
				break;
				case "BIT": case "SET": case "RES":
					if ((Match = (GetRegex(@"^([0-7]),(\(HL\)|A|B|C|D|E|H|L|IXh|IXl|IYh|IYl|%n|(\((IX|IY)\+%d\)))$")).Match(Param)).Success)
					{
						var Bit = int.Parse(Match.Groups[1].Value);
						var RightRegister = Match.Groups[2].Value;
						//Console.WriteLine("do" + Opcode + ":" + Bit + ":" + RightRegister);
						return ast.Statement(
							ast.CallStatic(
								(Action<CpuContext, int, byte>)Z80InterpreterImplementation.doBIT_r,
								GetCpuContext(), Bit, ParseRightRegister(RightRegister, Scope)
							)
						);
					}
				break;
				case "EI": case "DI":
					return ast.Statement(
						ast.CallStatic(
							((Action<CpuContext, bool>)Z80InterpreterImplementation.EnableDisableInterrupt),
							GetCpuContext(),
							(Opcode == "EI")
						)
					);

				case "IN":
					if ((Match = (GetRegex(@"^A,\(%n\)$")).Match(Param)).Success)
					{
						return ast.Statement(ast.CallStatic(
							((Action<CpuContext, byte>)Z80InterpreterImplementation.doIN),
							GetCpuContext(),
							ast.Cast<byte>(ast.Local(Scope.Get("%n")))
						));
					}
					break;
				case "OUT":
					if ((Match = (GetRegex(@"^\(%n\),A$")).Match(Param)).Success)
					{
						return ast.Statement(ast.CallStatic(
							((Action<CpuContext, byte>)Z80InterpreterImplementation.doOUT),
							GetCpuContext(),
							ast.Cast<byte>(ast.Local(Scope.Get("%n")))
						));
					}
					break;
				case "EX":
					if ((Match = (GetRegex(@"^DE,HL$")).Match(Param)).Success)
					{
						return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doEXDEHL), GetCpuContext()));
					}
					if ((Match = (GetRegex(@"^AF,AF'$")).Match(Param)).Success)
					{
						return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doEXAFAF_), GetCpuContext()));
					}
					break;
				case "DAA": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doDAA), GetCpuContext()));
				case "CPL": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doCPL), GetCpuContext()));
				case "RET": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doRET), GetCpuContext()));
				case "EXX": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doEXX),GetCpuContext()));
				case "OTIR": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doOTIR), GetCpuContext()));
				case "OUTI": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doOUTI), GetCpuContext()));
				case "LDI": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doLDI), GetCpuContext()));
				case "LDIR": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doLDIR), GetCpuContext()));
				case "SCF": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doSCF), GetCpuContext()));
				case "CCF": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doCCF), GetCpuContext()));
				case "HALT": return ast.Statement(ast.CallStatic(((Action<CpuContext>)Z80InterpreterImplementation.doHALT), GetCpuContext()));
				case "DJNZ": 
					if ((Match = (GetRegex(@"^\(PC\+%e\)$")).Match(Param)).Success)
					{
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, sbyte>)Z80InterpreterImplementation.doDJNZ),
								GetCpuContext(),
								ast.Cast<sbyte>(ast.Local(Scope.Get("%e")))
							)
						);
					}
					break;
				case "POP": 
					if ((Match = (GetRegex(@"^(AF|BC|DE|HL|IX|IY)$")).Match(Param)).Success)
					{
						var Register = Match.Groups[1].Value;
						return ast.Assign(
							GetRegister(Register),
							ast.CallStatic(
								((Func<CpuContext, ushort>)Z80InterpreterImplementation.doPop),
								GetCpuContext()
							)
						);
					}
					break;
				case "PUSH": 
					if ((Match = (GetRegex(@"^(AF|BC|DE|HL|IX|IY)$")).Match(Param)).Success)
					{
						var Register = Match.Groups[1].Value;
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, ushort>)Z80InterpreterImplementation.doPush),
								GetCpuContext(), GetRegister(Register)
							)
						);
					}
					break;
				case "INC":
				case "DEC":
					{
						int AddValue = (Opcode == "INC") ? 1 : -1;
						bool isDec = (Opcode == "DEC");

						//(INC|DEC) 
						if ((Match = (GetRegex(@"^\((IX|IY)\+%d\)$")).Match(Param)).Success)
						{
							var Register = Match.Groups[1].Value;
							//ctx->tstates += 6;
							//char off = read8(ctx, ctx->PC++);
							//byte value = read8(ctx, WR.%2 + off);
							//write8(ctx, WR.%2 + off, doIncDec(ctx, value, ID_%1));
							return WriteMemory1(
								GetRegister(Register) + GetDByte(Scope),
								ast.CallStatic(
									((Func<CpuContext, byte, bool, byte>)Z80InterpreterImplementation.doIncDec),
									GetCpuContext(), ReadMemory1(GetRegister(Register) + GetDByte(Scope)), isDec
								)
							);
						}

						if ((Match = (GetRegex(@"^\(HL\)$")).Match(Param)).Success)
						{
							//ctx->tstates += 1;
							//byte value = read8(ctx, WR.HL);
							//write8(ctx, WR.HL, doIncDec(ctx, value, ID_ % 1));
							return WriteMemory1(
								GetRegister("HL"),
								ast.CallStatic(
									((Func<CpuContext, byte, bool, byte>)Z80InterpreterImplementation.doIncDec),
									GetCpuContext(), ReadMemory1(GetRegister("HL")), isDec
								)
							);
						}
						if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl)$")).Match(Param)).Success)
						{
							var Register = Match.Groups[1].Value;
							return ast.Assign(
								GetRegister(Register),
								ast.CallStatic(
									((Func<CpuContext, byte, bool, byte>)Z80InterpreterImplementation.doIncDec),
									GetCpuContext(), ast.Cast<byte>(GetRegister(Register)), isDec
								)
							);
						}
						if ((Match = (GetRegex(@"^(BC|DE|HL|SP|IX|IY)$")).Match(Param)).Success)
						{
							var Register = Match.Groups[1].Value;
							return ast.Assign(GetRegister(Register), ast.Cast<ushort>(GetRegister(Register) + (ushort)AddValue));
						}
					}
					break;
				case "IM":
					if ((Match = (GetRegex(@"^([012])$")).Match(Param)).Success)
					{
						var Mode = int.Parse(Match.Groups[1].Value);
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, byte>)Z80InterpreterImplementation.InterruptMode),
								GetCpuContext(),
								ast.Cast<byte>(Mode)
							)
						);
					}
				break;
				// JumP
				case "JP":
					if ((Match = (GetRegex(@"^\((HL|IX|IY)\)$")).Match(Param)).Success)
					{
						var Register = Match.Groups[1].Value;
						//ctx->PC = WR.%1;
						return ast.Assign(GetRegister("PC"), GetRegister(Register));
					}
					if ((Match = (GetRegex(@"^(C|M|NZ|NC|P|PE|PO|Z)?,?\(%nn\)$")).Match(Param)).Success)
					{
						var Flag = Match.Groups[1].Value;
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, bool, ushort>)Z80InterpreterImplementation.doJUMP),
								GetCpuContext(), GetFlag(Flag), GetNNWord(Scope)
							)
						);
					}
				break;
				// Jump Relative
				case "JR":
					if ((Match = (GetRegex(@"^(C|NZ|NC|Z)?,?\(PC\+%e\)$")).Match(Param)).Success)
					{
						var Flag = Match.Groups[1].Value;
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, bool, sbyte>)Z80InterpreterImplementation.doJUMP_Inc),
								GetCpuContext(), GetFlag(Flag), ast.Cast<sbyte>(ast.Local(Scope.Get("%e")))
							)
						);
					}
				break;
				case "RST":
					if ((Match = (GetRegex(@"^(0|8|10|18|20|28|30|38)H$")).Match(Param)).Success)
					{
						var PC = Convert.ToInt32(Match.Groups[1].Value, 16);
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, byte>)Z80InterpreterImplementation.doRST),
								GetCpuContext(), (byte)PC
							)
						);
					}
				break;
				case "CALL":
					if ((Match = (GetRegex(@"^(C|M|NZ|NC|P|PE|PO|Z)?,?\(%nn\)$")).Match(Param)).Success)
					{
						var Flag = Match.Groups[1].Value;
						return ast.Statement(
							ast.CallStatic(
								((Action<CpuContext, bool, ushort>)Z80InterpreterImplementation.doCALL),
								GetCpuContext(), GetFlag(Flag), GetNNWord(Scope)
							)
						);
						//ushort addr = read16(ctx, ctx->PC);
						//ctx->PC += 2;
						//if (condition(ctx, C_ % 1))
						//{
						//	ctx->tstates += 1;
						//	doPush(ctx, ctx->PC);
						//	ctx->PC = addr;
						//}
					}
				break;
				case "NOP": return ast.Statement();
				// LOAD
				case "LD":
					if ((Match = (GetRegex(@"^\((IX|IY)\+%d\),(A|B|C|D|E|H|L)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						var RightRegister = Match.Groups[2].Value;
						return WriteMemory1(
							GetRegister(LeftRegister) + GetDByte(Scope),
							GetRegister(RightRegister)
						);
					}

					if ((Match = (GetRegex(@"^\((IX|IY)\+%d\),%n$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return WriteMemory1(GetRegister(LeftRegister) + GetDByte(Scope), GetNByte(Scope));
					}

					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L),(\((IX|IY)\+%d\))$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						var RightRegister = Match.Groups[2].Value;
						return ast.Assign(GetRegister(LeftRegister), ParseRightRegister(RightRegister, Scope));
					}

					if ((Match = (GetRegex(@"^(BC|DE|HL|SP|IX|IY),\(%nn\)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Assign(GetRegister(LeftRegister), ReadMemory2(GetNNWord(Scope)));
					}

					if ((Match = (GetRegex(@"^A,\((BC|DE)\)$")).Match(Param)).Success)
					{
						var RightRegister = Match.Groups[1].Value;
						return ast.Assign(GetRegister("A"), ReadMemory1(GetRegister(RightRegister)));
					}

					if ((Match = (GetRegex(@"^\(HL\),%n$")).Match(Param)).Success)
					{
						return WriteMemory1(GetRegister("HL"), GetNByte(Scope));
					}

					if ((Match = (GetRegex(@"^\(%nn\),(BC|DE|HL|IX|IY|SP)$")).Match(Param)).Success)
					{
						var RightRegister = Match.Groups[1].Value;
						return WriteMemory2(GetNNWord(Scope), GetRegister(RightRegister));
					}

					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L),\(%nn\)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Assign(GetRegister(LeftRegister), ast.Cast<byte>(ReadMemory1(GetNNWord(Scope))));
					}

					if ((Match = (GetRegex(@"^\((BC|DE|HL)\),(A|B|C|D|E|H|L)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						var RightRegister = Match.Groups[2].Value;
						return WriteMemory1(GetRegister(LeftRegister), GetRegister(RightRegister));
					}

					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L),\(HL\)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Assign(GetRegister(LeftRegister), ReadMemory1(GetRegister("HL")));
					}

					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl),(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl)$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						var RightRegister = Match.Groups[2].Value;
						return ast.Assign(GetRegister(LeftRegister), GetRegister(RightRegister));
					}

					if ((Match = (GetRegex(@"^(BC|DE|HL|SP|IX|IY),%nn$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Statements(
							ast.Assign(
								GetRegister(LeftRegister),
								GetNNWord(Scope)
							)
						);
					}
					if ((Match = (GetRegex(@"^(A|B|C|D|E|H|L|IXh|IXl|IYh|IYl),%n$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Statements(
							ast.Assign(
								GetRegister(LeftRegister),
								GetNByte(Scope)
							)
						);
					}
					if ((Match = (GetRegex(@"^\(%nn\),A$")).Match(Param)).Success)
					{
						var LeftRegister = Match.Groups[1].Value;
						return ast.Statements(
							ast.Statement(ast.CallInstance(
								GetCpuContext(),
								((Action<ushort, byte>)CpuContext._NullInstance.WriteMemory1).Method,
								GetNNWord(Scope), GetRegister("A")
							))
						);
					}
					if ((Match = (GetRegex(@"^\(HL\),(B|C|D|E|H|L)$")).Match(Param)).Success)
					{
						var RightRegister = Match.Groups[1].Value;
						return ast.Statements(
							ast.Statement(ast.CallInstance(
								GetCpuContext(),
								((Action<ushort, byte>)CpuContext._NullInstance.WriteMemory1).Method,
								GetRegister("HL"), GetRegister(RightRegister)
							))
						);
					}
				break;
			}
			return null;
		}

		static private InstructionInfo Instruction(string OpCode, string Mnemonic, int tstates = 1, int tstatesIfNoBranch = 1)
		{
			Mnemonic = Mnemonic.Trim();
			var MnemonicParts = Mnemonic.Split(new[] { ' ' }, 2);
			var MnemonicName = (MnemonicParts.Length > 0) ? MnemonicParts[0] : "";
			var MnemonicFormat = (MnemonicParts.Length > 1) ? MnemonicParts[1] : "";

			//var Scope = new Scope<string,AstLocal>();
			//Scope.Set("%d", AstLocal.Create<byte>("VAR_D"));
			//Scope.Set("%n", AstLocal.Create<byte>("VAR_N"));
			//Console.WriteLine(GeneratorCSharp.GenerateString(Process(Mnemonic, Scope)));

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
			return new InstructionInfo(MnemonicName, MnemonicFormat, MaskDataVarsList)
			{
				Tstates = tstates,
				TstatesIfNoBranch = tstatesIfNoBranch,
			};
		}
	}
}
