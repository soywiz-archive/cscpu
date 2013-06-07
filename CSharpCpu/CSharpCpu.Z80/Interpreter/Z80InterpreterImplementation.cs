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
			Context.R1.A = 1;
		}

		static public void _Test2(CpuContext Context)
		{
			var Z = Context.R1.A;
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
			ctx.R1.A &= value;
			adjustLogicFlag(ctx, true);
		}


		static public void doOR(CpuContext ctx, byte value)
		{
			ctx.R1.A |= value;
			adjustLogicFlag(ctx, false);
		}


		static public void doXOR(CpuContext ctx, byte value)
		{
			ctx.R1.A ^= value;
			adjustLogicFlag(ctx, false);
		}

		/* Adjust flags after AND, OR, XOR */
		static void adjustLogicFlag(CpuContext ctx, bool flagH)
		{
			ctx.VALFLAG(Z80Flags.F_S, (ctx.R1.A & 0x80) != 0);
			ctx.VALFLAG(Z80Flags.F_Z, (ctx.R1.A == 0));
			ctx.VALFLAG(Z80Flags.F_H, flagH);
			ctx.VALFLAG(Z80Flags.F_N, false);
			ctx.VALFLAG(Z80Flags.F_C, false);
			ctx.VALFLAG(Z80Flags.F_PV, parityBit[ctx.R1.A]);

			adjustFlags(ctx, ctx.R1.A);
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

		static void adjustFlags(CpuContext ctx, byte val)
		{
			ctx.VALFLAG(Z80Flags.F_5, (val & (byte)Z80Flags.F_5) != 0);
			ctx.VALFLAG(Z80Flags.F_3, (val & (byte)Z80Flags.F_3) != 0);
		}

		public static void Jump(CpuContext ctx, bool JumpIf, ushort Address)
		{
			if (JumpIf) ctx.PC = Address;
		}

		public static void JumpInc(CpuContext ctx, bool JumpIf, sbyte Address)
		{
			if (JumpIf) ctx.PC = (ushort)(ctx.PC + Address);
		}
	}
}
