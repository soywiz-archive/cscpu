using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Decoder
{
	sealed public class SwitchGenerator
	{
		static public AstNodeStm GenerateSwitch<TDecoderReference>(IEnumerable<TDecoderReference> AllItems, Func<TDecoderReference, AstNodeExpr> Process, uint BaseMask = 0xFFFFFFFF) where TDecoderReference : IDecoderReference
		{
			return new SwitchGeneratorInternal<TDecoderReference>() {
				Local = AstLocal.Create(typeof(uint), "CurrentWord"),
				ReaderArgument = new AstArgument(0, typeof(Func<uint>), "Read"),
				AllItems = AllItems,
				Process = Process,
			}.GenerateSwitch(BaseMask);
		}

		sealed internal class SwitchGeneratorInternal<TDecoderReference> where TDecoderReference : IDecoderReference
		{
			internal AstLocal Local;
			internal AstArgument ReaderArgument;
			internal Func<TDecoderReference, AstNodeExpr> Process;
			internal IEnumerable<TDecoderReference> AllItems;

			internal AstNodeStm GenerateSwitch(uint BaseMask = 0xFFFFFFFF)
			{

				return new AstNodeStmContainer(
					new AstNodeStmAssign(new AstNodeExprLocal(Local), new AstNodeExprCallDelegate(new AstNodeExprArgument(ReaderArgument))),
					_GenerateSwitch(BaseMask, AllItems, 0, 0),
					new AstNodeStmReturn(Process(default(TDecoderReference)))
				);
			}

			private uint GetCommonMask(uint BaseMask, IEnumerable<TDecoderReference> Items, int ReferenceIndex)
			{
				return Items.Aggregate(BaseMask, (CurrentMask, Item) => CurrentMask & Item.Mask[ReferenceIndex]);
			}

			public int GetShiftRightMask(uint Mask)
			{
				int Count = 0;
				while (Mask != 0 && ((Mask & 1) == 0)) { Mask >>= 1; Count++; }
				return Count;
			}

			private AstNodeStm _GenerateSwitch(uint BaseMask, IEnumerable<TDecoderReference> Items, int ReferenceIndex, int NestLevel)
			{
				if (Items == null) Items = AllItems;
				if (NestLevel > 10) throw(new Exception("Too much nesting. Probably an error."));

				//Console.WriteLine("------------------------");

				Items = Items.Where(Item => Item.Mask.Length > ReferenceIndex);

				var CommonMask = GetCommonMask(BaseMask, Items, ReferenceIndex);
				var CommonShift = GetShiftRightMask(CommonMask);

				//Console.WriteLine("BaseMask: 0x{0:X8}", BaseMask);
				//Console.WriteLine("CommonMask: 0x{0:X8} | 0x{1:X8}", CommonMask, ~CommonMask);

				var Cases = new List<AstNodeCase>();

				foreach (var Group in Items.GroupBy(Item => Item.Data[ReferenceIndex] & CommonMask))
				{
					AstNodeStm CaseBody;
					var GroupMask = GetCommonMask(BaseMask, Group, ReferenceIndex);

					//Console.WriteLine("  GroupMask: 0x{0:X8}, 0x{1:X8}", GroupMask, GroupMask & ~CommonMask);

					if ((Group.Count() == 1) && ((GroupMask & ~CommonMask) == 0))
					{
						// Leaf.
						if (ReferenceIndex < Group.First().Data.Length - 1)
						{
							//new AstNodeExprLocal(
							CaseBody = new AstNodeStmContainer(
								new AstNodeStmAssign(new AstNodeExprLocal(Local), new AstNodeExprCallDelegate(new AstNodeExprArgument(ReaderArgument))),
								_GenerateSwitch(0xFFFFFFFF, null, ReferenceIndex + 1, NestLevel + 1)
							);
						}
						else
						{
							CaseBody = new AstNodeStmReturn(Process(Group.First()));
						}
					}
					else
					{
						CaseBody = _GenerateSwitch(BaseMask & ~CommonMask, Group, ReferenceIndex, NestLevel + 1);
						//CaseBody = new AstNodeStmReturn(null);
					}

					Cases.Add(new AstNodeCase(((Group.First().Data[ReferenceIndex] >> CommonShift) & (CommonMask >> CommonShift)), CaseBody));
				}

				return new AstNodeStmSwitch(
					new AstNodeExprBinop((new AstNodeExprLocal(Local)), ">>", CommonShift) & (CommonMask >> CommonShift),
					Cases,
					new AstNodeCaseDefault(new AstNodeStmReturn(Process(default(TDecoderReference))))
				);
			}
		}
	}
}
