﻿using SafeILGenerator.Ast;
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
				BaseMask = BaseMask,
			}.GenerateSwitch(AllItems, 0);
		}

		sealed internal class SwitchGeneratorInternal<TDecoderReference> where TDecoderReference : IDecoderReference
		{
			internal AstLocal Local;
			internal AstArgument ReaderArgument;
			internal Func<TDecoderReference, AstNodeExpr> Process;
			internal IEnumerable<TDecoderReference> AllItems;
			internal uint BaseMask;

			internal AstNodeStm GenerateSwitch(IEnumerable<TDecoderReference> Items, int ReferenceIndex)
			{
				return new AstNodeStmContainer(
					new AstNodeStmAssign(new AstNodeExprLocal(Local), new AstNodeExprCallDelegate(new AstNodeExprArgument(ReaderArgument))),
					_GenerateSwitch(this.BaseMask, Items, ReferenceIndex, 0),
					new AstNodeStmReturn(Process(default(TDecoderReference)))
				);
			}

			private uint GetCommonMask(uint BaseMask, IEnumerable<TDecoderReference> Items, int ReferenceIndex)
			{
				return Items.Aggregate(BaseMask, (CurrentMask, Item) => CurrentMask & Item.MaskDataVars[ReferenceIndex].Mask);
			}

			public int GetShiftRightMask(uint Mask)
			{
				int Count = 0;
				while (Mask != 0 && ((Mask & 1) == 0)) { Mask >>= 1; Count++; }
				return Count;
			}

			private AstNodeStm _GenerateSwitch(uint BaseMask, IEnumerable<TDecoderReference> ItemsBase, int ReferenceIndex, int NestLevel)
			{
				if (ItemsBase == null) ItemsBase = AllItems;
				if (NestLevel > 64) throw(new Exception("Too much nesting. Probably an error."));

				//Console.WriteLine("------------------------");

				var Items = ItemsBase.Where(Item => Item.MaskDataVars.Length > ReferenceIndex);

				if (Items.Count() == 0)
				{
					if (ItemsBase.Count() == 1)
					{
						return new AstNodeStmReturn(Process(ItemsBase.First()));
					}
					else
					{
						throw (new Exception("Unexpected case"));
						//return new AstNodeStmEmpty();
					}
				}

				int MaxItemsLength = Items.Max(Item => Item.MaskDataVars.Length);

				var CommonMask = GetCommonMask(BaseMask, Items, ReferenceIndex);
				var CommonShift = GetShiftRightMask(CommonMask);

				//Console.WriteLine("{0}: {1}", CommonMask, String.Join(",", Items));

				if (CommonMask == 0)
				{
					return GenerateSwitch(Items, ReferenceIndex + 1);

					/*
					if (ReferenceIndex + 1 >= MaxItemsLength)
					{
						if (Items.Count() == 1)
						{
							return new AstNodeStmReturn(Process(Items.First()));
						}
						else
						{
							throw(new Exception("Unexpected case"));
						}
					}
					else
					{
						return GenerateSwitch(Items, ReferenceIndex + 1);
					}
					*/
				}

				//Console.WriteLine("BaseMask: 0x{0:X8}", BaseMask);
				//Console.WriteLine("CommonMask: 0x{0:X8} | 0x{1:X8}", CommonMask, ~CommonMask);

				var Cases = new List<AstNodeCase>();

				foreach (var Group in Items.GroupBy(Item => Item.MaskDataVars[ReferenceIndex].Data & CommonMask))
				{
					AstNodeStm CaseBody;
					var GroupMask = GetCommonMask(BaseMask, Group, ReferenceIndex);

					//Console.WriteLine("  GroupMask: 0x{0:X8}, 0x{1:X8}", GroupMask, GroupMask & ~CommonMask);

					if ((Group.Count() == 1) && ((GroupMask & ~CommonMask) == 0))
					{
						// Leaf.
						if (ReferenceIndex < Group.First().MaskDataVars.Length - 1)
						{
							//new AstNodeExprLocal(
							CaseBody = GenerateSwitch(Group, ReferenceIndex + 1);
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

					Cases.Add(new AstNodeCase(((Group.First().MaskDataVars[ReferenceIndex].Data >> CommonShift) & (CommonMask >> CommonShift)), CaseBody));
				}

				return new AstNodeStmSwitch(
					new AstNodeExprBinop((new AstNodeExprLocal(Local)), ">>", CommonShift) & (CommonMask >> CommonShift)
					, Cases
					//, new AstNodeCaseDefault(new AstNodeStmReturn(Process(default(TDecoderReference))))
				);
			}
		}
	}
}
