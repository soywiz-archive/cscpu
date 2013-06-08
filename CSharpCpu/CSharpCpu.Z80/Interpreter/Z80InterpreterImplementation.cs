using CSharpCpu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Z80.Interpreter
{
	public class Z80InterpreterImplementation
	{
		static public void INVALID(CpuContext Context)
		{
			throw(new InvalidOperationException("Invalid instruction"));
		}

		static public void _Test1(CpuContext Context)
		{
			Context.A = 1;
		}

		static public void _Test2(CpuContext Context)
		{
			var Z = Context.A;
		}

		static public void _Test3(CpuContext Context)
		{
			Context.AF = 7;
		}

		static public void _Test4(CpuContext Context)
		{
			var Test = Context.AF;
		}

		static public void OutputDebug(byte Value)
		{
			Console.WriteLine("OutputDebug: {0:X2}", Value);
		}

		static public void UNIMPLEMENTED(CpuContext Context, string Name, string Format)
		{
			Console.WriteLine("Not Implemented Instruction '{0}' '{1}' at 0x{2:X4}", Name, Format, Context.PC);
			Console.ReadKey();
			throw (new NotImplementedException(String.Format("Not Implemented Instruction '{0}' '{1}' at 0x{2:X4}", Name, Format, Context.PC)));
		}

		static public void InterruptMode(CpuContext ctx, byte Mode)
		{
			ctx.IM = Mode;
		}

		static public void EnableDisableInterrupt(CpuContext ctx, bool Enable)
		{
			ctx.IFF1 = ctx.IFF2 = Enable;
			ctx.defer_int = true;
		}

		static public ushort doArithmeticWord(CpuContext ctx, ushort a1, ushort a2, bool withCarry, bool isSub)
		{
			if (withCarry && ctx.GETFLAG(Z80Flags.F_C))
				a2++;
			int sum = a1;
			if (isSub)
			{
				sum -= a2;
				ctx.VALFLAG(Z80Flags.F_H, (((a1 & 0x0fff) - (a2 & 0x0fff)) & 0x1000) != 0);
			}
			else
			{
				sum += a2;
				ctx.VALFLAG(Z80Flags.F_H, (((a1 & 0x0fff) + (a2 & 0x0fff)) & 0x1000) != 0);
			}
			ctx.VALFLAG(Z80Flags.F_C, (sum & 0x10000) != 0);
			if (withCarry || isSub)
			{
				int minuend_sign = a1 & 0x8000;
				int subtrahend_sign = a2 & 0x8000;
				int result_sign = sum & 0x8000;
				bool overflow;
				if (isSub)
				{
					overflow = minuend_sign != subtrahend_sign && result_sign != minuend_sign;
				}
				else
				{
					overflow = minuend_sign == subtrahend_sign && result_sign != minuend_sign;
				}
				ctx.VALFLAG(Z80Flags.F_PV, overflow);
				ctx.VALFLAG(Z80Flags.F_S, (sum & 0x8000) != 0);
				ctx.VALFLAG(Z80Flags.F_Z, sum == 0);
			}
			ctx.VALFLAG(Z80Flags.F_N, isSub);
			adjustFlags(ctx, (byte)(sum >> 8));
			return (ushort)sum;
		}

		static public byte doArithmeticByte(CpuContext ctx, byte a1, byte a2, bool withCarry, bool isSub)
		{
			ushort res; /* To detect carry */

			if (isSub)
			{
				ctx.SETFLAG(Z80Flags.F_N);
				ctx.VALFLAG(Z80Flags.F_H, (((a1 & 0x0F) - (a2 & 0x0F)) & 0x10) != 0);
				res = (ushort)(a1 - a2);
				if (withCarry && ctx.GETFLAG(Z80Flags.F_C)) res--;
			}
			else
			{
				ctx.RESFLAG(Z80Flags.F_N);
				ctx.VALFLAG(Z80Flags.F_H, (((a1 & 0x0F) + (a2 & 0x0F)) & 0x10) != 0);
				res = (ushort)(a1 + a2);
				if (withCarry && ctx.GETFLAG(Z80Flags.F_C)) res++;
			}
			ctx.VALFLAG(Z80Flags.F_S, ((res & 0x80) != 0));
			ctx.VALFLAG(Z80Flags.F_C, ((res & 0x100) != 0));
			ctx.VALFLAG(Z80Flags.F_Z, ((res & 0xff) == 0));
			int minuend_sign = a1 & 0x80;
			int subtrahend_sign = a2 & 0x80;
			int result_sign = res & 0x80;
			bool overflow;
			if (isSub)
			{
				overflow = minuend_sign != subtrahend_sign && result_sign != minuend_sign;
			}
			else
			{
				overflow = minuend_sign == subtrahend_sign && result_sign != minuend_sign;
			}
			ctx.VALFLAG(Z80Flags.F_PV, overflow);
			adjustFlags(ctx, (byte)res);

			return (byte)(res & 0xFF);
		}

		static public void doAND(CpuContext ctx, byte value)
		{
			ctx.A &= value;
			adjustLogicFlag(ctx, true);
		}


		static public void doOR(CpuContext ctx, byte value)
		{
			ctx.A |= value;
			adjustLogicFlag(ctx, false);
		}


		static public void doXOR(CpuContext ctx, byte value)
		{
			ctx.A ^= value;
			adjustLogicFlag(ctx, false);
		}

		/* Adjust flags after AND, OR, XOR */
		static void adjustLogicFlag(CpuContext ctx, bool flagH)
		{
			ctx.VALFLAG(Z80Flags.F_S, (ctx.A & 0x80) != 0);
			ctx.VALFLAG(Z80Flags.F_Z, (ctx.A == 0));
			ctx.VALFLAG(Z80Flags.F_H, flagH);
			ctx.VALFLAG(Z80Flags.F_N, false);
			ctx.VALFLAG(Z80Flags.F_C, false);
			ctx.VALFLAG(Z80Flags.F_PV, parityBit[ctx.A]);

			adjustFlags(ctx, ctx.A);
		}

		static readonly bool[] parityBit = new byte[256] { 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1,
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 
			1, 0, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 1
		}.Select(Item => Item != 0).ToArray();

		public static void adjustFlags(CpuContext ctx, byte val)
		{
			ctx.VALFLAG(Z80Flags.F_5, (val & (byte)Z80Flags.F_5) != 0);
			ctx.VALFLAG(Z80Flags.F_3, (val & (byte)Z80Flags.F_3) != 0);
		}

		static public void doPush(CpuContext ctx, ushort val)
		{
			ctx.SP -= 2;
			ctx.WriteMemory2(ctx.SP, val);
		}

		static public ushort doPop(CpuContext ctx)
		{
			ushort val = ctx.ReadMemory2(ctx.SP);
			ctx.SP += 2;
			return val;
		}

		public static void doRET(CpuContext ctx)
		{
			ctx.PC = doPop(ctx);
		}

		public static void doCALL(CpuContext ctx, bool JumpIf, ushort Address)
		{
			if (JumpIf)
			{
				doPush(ctx, ctx.PC);
				ctx.PC = Address;
			}
		}

		public static void doJUMP(CpuContext ctx, bool JumpIf, ushort Address)
		{
			if (JumpIf) ctx.PC = Address;
		}

		public static void doJUMP_Inc(CpuContext ctx, bool JumpIf, sbyte Address)
		{
			if (JumpIf) ctx.PC = (ushort)(ctx.PC + Address);
		}

		public static void doEXX(CpuContext ctx)
		{
			ctx.BC = LangUtils.SwapRet(ctx.BC, ref ctx.BC_);
			ctx.DE = LangUtils.SwapRet(ctx.DE, ref ctx.DE_);
			ctx.HL = LangUtils.SwapRet(ctx.HL, ref ctx.HL_);
		}

		public static byte doIncDec(CpuContext ctx, byte val, bool isDec)
		{
			if (isDec)
			{
				ctx.VALFLAG(Z80Flags.F_PV, ((val & 0x80) != 0) && !(((val - 1) & 0x80) != 0));
				val--;
				ctx.VALFLAG(Z80Flags.F_H, (val & 0x0F) == 0x0F);
			}
			else
			{
				ctx.VALFLAG(Z80Flags.F_PV, !((val & 0x80) != 0) && (((val + 1) & 0x80) != 0));
				val++;
				ctx.VALFLAG(Z80Flags.F_H, !((val & 0x0F) != 0));
			}

			ctx.VALFLAG(Z80Flags.F_S, ((val & 0x80) != 0));
			ctx.VALFLAG(Z80Flags.F_Z, (val == 0));
			ctx.VALFLAG(Z80Flags.F_N, isDec);

			adjustFlags(ctx, val);

			return val;
		}

		public static void doOTIR(CpuContext ctx)
		{
#if true
			while (ctx.B != 0) doOUTI(ctx);
#else
			doOUTI(ctx);
			if (ctx.B != 0) ctx.PC -= 2;
#endif
		}

		public static void doOUTI(CpuContext ctx)
		{
			var value = ctx.ReadMemory1(ctx.HL);
			ctx.B = doIncDec(ctx, ctx.B, true);
			ctx.ioWrite(ctx.BC, value);
			ctx.HL++;
			int flag_value = value + ctx.L;
			ctx.VALFLAG(Z80Flags.F_N, (value & 0x80) != 0);
			ctx.VALFLAG(Z80Flags.F_H, flag_value > 0xff);
			ctx.VALFLAG(Z80Flags.F_C, flag_value > 0xff);
			ctx.VALFLAG(Z80Flags.F_PV, parityBit[(flag_value & 7) ^ ctx.B]);
			adjustFlags(ctx, ctx.B);
		}

		public static void doLDI(CpuContext ctx)
		{
			//ctx->tstates += 2;
			byte val = ctx.ReadMemory1(ctx.HL);
			ctx.WriteMemory1(ctx.DE, val);
			ctx.DE++;
			ctx.HL++;
			ctx.BC--;
			ctx.VALFLAG(Z80Flags.F_5, ((ctx.A + val) & 0x02) != 0);
			ctx.VALFLAG(Z80Flags.F_3, ((Z80Flags)(ctx.A + val) & Z80Flags.F_3) != 0);
			ctx.RESFLAG(Z80Flags.F_H | Z80Flags.F_N);
			ctx.VALFLAG(Z80Flags.F_PV, ctx.BC != 0);
		}

		// LDI Repeat
		public static void doLDIR(CpuContext ctx)
		{
#if true
			while (ctx.BC != 0) doLDI(ctx);
#else
			doLDI(ctx);
			if (ctx.BC != 0) ctx.PC -= 2;
#endif
		}

		public static void doDJNZ(CpuContext ctx, sbyte Offset)
		{
			//ctx.tstates += 1;
			ctx.B--;
			if (ctx.B != 0)
			{
				//ctx->tstates += 5;
				ctx.PC = (ushort)(ctx.PC + Offset);
			}
		}

		public static void doOUT(CpuContext ctx, byte Port)
		{
			ctx.ioWrite((ushort)(ctx.A << 8 | Port), ctx.A);
		}

		public static void doIN(CpuContext ctx, byte Port)
		{
			//IN A,\(n\)
			//	byte port = read8(ctx, ctx->PC++);	
			//	BR.A = ioRead(ctx, BR.A << 8 | port);
			ctx.A = ctx.ioRead((ushort)((ctx.A << 8) | Port));
		}

		public static void doRST(CpuContext ctx, byte nPC)
		{
			//ctx->tstates += 1;
			doPush(ctx, ctx.PC);
			ctx.PC = nPC;
		}

		public static void doBIT(CpuContext ctx, int Bit, byte Value)
		{
			if ((Value & (1 << Bit)) != 0)
			{
				ctx.RESFLAG(Z80Flags.F_Z | Z80Flags.F_PV);
			}
			else
			{
				ctx.SETFLAG(Z80Flags.F_Z | Z80Flags.F_PV);
			}

			ctx.SETFLAG(Z80Flags.F_H);
			ctx.RESFLAG(Z80Flags.F_N);

			ctx.RESFLAG(Z80Flags.F_S);
			if ((Bit == 7) && !ctx.GETFLAG(Z80Flags.F_Z))
			{
				ctx.SETFLAG(Z80Flags.F_S);
			}
		}

		public static void doBIT_r(CpuContext ctx, int Bit, byte Value)
		{
			doBIT(ctx, Bit, Value);
			ctx.VALFLAG(Z80Flags.F_5, ((Z80Flags)Value & Z80Flags.F_5) != 0);
			ctx.VALFLAG(Z80Flags.F_3, ((Z80Flags)Value & Z80Flags.F_3) != 0);
		}

		public static void doEXAFAF_(CpuContext ctx)
		{
			ctx.AF = LangUtils.SwapRet(ctx.AF, ref ctx.AF_);
		}

		static void adjustFlagSZP(CpuContext ctx, byte val)
		{
			ctx.VALFLAG(Z80Flags.F_S, (val & 0x80) != 0);
			ctx.VALFLAG(Z80Flags.F_Z, (val == 0));
			ctx.VALFLAG(Z80Flags.F_PV, parityBit[val]);
		}

		public static byte doRLC(CpuContext ctx, bool adjFlags, byte val)
		{
			ctx.VALFLAG(Z80Flags.F_C, (val & 0x80) != 0);
			val <<= 1;
			val |= (byte)(ctx.GETFLAG(Z80Flags.F_C) ? 1 : 0);

			adjustFlags(ctx, val);
			ctx.RESFLAG(Z80Flags.F_H | Z80Flags.F_N);

			if (adjFlags)
				adjustFlagSZP(ctx, val);

			return val;
		}

		public static byte doRL(CpuContext ctx, bool adjFlags, byte val)
		{
			int CY = ctx.GETFLAG(Z80Flags.F_C) ? 1 :0;
			ctx.VALFLAG(Z80Flags.F_C, (val & 0x80) != 0);
			val <<= 1;
			val |= (byte)CY;

			adjustFlags(ctx, val);
			ctx.RESFLAG(Z80Flags.F_H | Z80Flags.F_N);

			if (adjFlags)
				adjustFlagSZP(ctx, val);

			return val;
		}

		public static byte doRRC(CpuContext ctx, bool adjFlags, byte val)
		{
			ctx.VALFLAG(Z80Flags.F_C, (val & 0x01) != 0);
			val >>= 1;
			val |= (byte)((ctx.GETFLAG(Z80Flags.F_C) ? 1 :0) << 7);

			adjustFlags(ctx, val);
			ctx.RESFLAG(Z80Flags.F_H | Z80Flags.F_N);

			if (adjFlags)
				adjustFlagSZP(ctx, val);

			return val;
		}


		public static byte doRR(CpuContext ctx, bool adjFlags, byte val)
		{
			int CY = ctx.GETFLAG(Z80Flags.F_C) ? 1 : 0;
			ctx.VALFLAG(Z80Flags.F_C, (val & 0x01) != 0);
			val >>= 1;
			val |= (byte)(CY << 7);

			adjustFlags(ctx, val);
			ctx.RESFLAG(Z80Flags.F_H | Z80Flags.F_N);

			if (adjFlags)
				adjustFlagSZP(ctx, val);

			return val;
		}

		public static byte doCP_HL(CpuContext ctx)
		{
			byte val = ctx.ReadMemory1(ctx.HL);
			byte result = doArithmeticByte(ctx, ctx.A, val, false, true);
			adjustFlags(ctx, val);
			return result;
		}

		public static void doCPL(CpuContext ctx)
		{
			ctx.A = (byte)(~ctx.A);
			ctx.SETFLAG(Z80Flags.F_H | Z80Flags.F_N);
			adjustFlags(ctx, ctx.A);
		}

		public static void doEXDEHL(CpuContext ctx)
		{
			var Temp = ctx.DE;
			ctx.DE = ctx.HL;
			ctx.HL = Temp;
		}
	}
}
