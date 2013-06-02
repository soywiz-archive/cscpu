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
		public class DecoderContext<TDecoderReference>
		{
			public TDecoderReference DecoderReference;
			public Scope<string, AstLocal> Scope;

			internal DecoderContext()
			{
			}
		}

		static public AstNodeStm GenerateSwitch<TDecoderReference>(IEnumerable<TDecoderReference> AllItems, Func<DecoderContext<TDecoderReference>, AstNodeExpr> Process, uint BaseMask = 0xFFFFFFFF) where TDecoderReference : IDecoderReference
		{
			return new SwitchGeneratorInternal<TDecoderReference>() {
				ReadedLocal = AstLocal.Create(typeof(uint), "CurrentWord"),
				ReaderArgument = new AstArgument(0, typeof(Func<uint>), "Read"),
				AllItems = AllItems,
				Process = Process,
				BaseMask = BaseMask,
			}.GenerateSwitch(AllItems, 0);
		}

		sealed internal class SwitchGeneratorInternal<TDecoderReference> where TDecoderReference : IDecoderReference
		{
			internal AstLocal ReadedLocal;
			internal AstArgument ReaderArgument;
			internal Func<DecoderContext<TDecoderReference>, AstNodeExpr> Process;
			internal IEnumerable<TDecoderReference> AllItems;
			internal uint BaseMask;
			internal Scope<string, AstLocal> LocalScope = new Scope<string, AstLocal>();

			internal AstNodeStm GenerateSwitch(IEnumerable<TDecoderReference> Items, int ReferenceIndex)
			{
				return LocalScope.CreateScope(() => {
					var Statements = new List<AstNodeStm>();
					Statements.Add(new AstNodeStmAssign(new AstNodeExprLocal(ReadedLocal), new AstNodeExprCallDelegate(new AstNodeExprArgument(ReaderArgument))));
					Statements.Add(_GenerateSwitch(this.BaseMask, Items, ReferenceIndex, 0));
					Statements.Add(_GenerateLeaf(default(TDecoderReference), ReferenceIndex));
					return new AstNodeStmContainer(Statements);
				});
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

			private AstNodeStm _GenerateLeaf(TDecoderReference DecoderReference, int ReferenceIndex)
			{
				//if (DecoderReference.MaskDataVars != null && ReferenceIndex < DecoderReference.MaskDataVars.Length && DecoderReference.MaskDataVars[ReferenceIndex].Vars.Length > 0)
				//{
				//	throw(new Exception("aaa!!"));
				//}

				var Stats = new AstNodeStmContainer();

				if (DecoderReference != null && DecoderReference.MaskDataVars != null)
				{
					if (ReferenceIndex < DecoderReference.MaskDataVars.Length)
					{
						if (DecoderReference.MaskDataVars[ReferenceIndex].Vars.Length != 0)
						{
							foreach (var Var in DecoderReference.MaskDataVars[ReferenceIndex].Vars)
							{
								var Local = AstLocal.Create(typeof(uint), Var.Name);
								LocalScope.Set(Var.Name, Local);
								Stats.AddStatement(new AstNodeStmAssign(
									new AstNodeExprLocal(Local),
									new AstNodeExprBinop(new AstNodeExprBinop(new AstNodeExprLocal(ReadedLocal), ">>", Var.Shift), "&", Var.Mask)
								));
							}
							//LocalScope.Set(
							//Console.WriteLine("{0}, {1}", ReferenceIndex, DecoderReference);
						}
					}
				}

				Stats.AddStatement(new AstNodeStmReturn(Process(
					new DecoderContext<TDecoderReference>()
					{
						DecoderReference = DecoderReference,
						Scope = LocalScope,
					}
				)));

				return Stats;
			}

			private AstNodeStm _GenerateSwitch(uint BaseMask, IEnumerable<TDecoderReference> ItemsBase, int ReferenceIndex, int NestLevel)
			{
				if (ItemsBase == null) ItemsBase = AllItems;
				if (NestLevel > 16) throw(new Exception("Too much nesting. Probably an error."));

				//Console.WriteLine("------------------------");

				var Items = ItemsBase.Where(Item => Item.MaskDataVars.Length > ReferenceIndex);

				if (Items.Count() == 0)
				{
					//if (ItemsBase.Count() == 1)
					//{
					//	return _GenerateLeaf(ItemsBase.First(), ReferenceIndex);
					//}
					//else
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
					if (Items.Count() == 1)
					{
						return _GenerateLeaf(ItemsBase.First(), ReferenceIndex);
					}
					return GenerateSwitch(Items, ReferenceIndex + 1);
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
							CaseBody = _GenerateLeaf(Group.First(), ReferenceIndex);
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
					new AstNodeExprBinop((new AstNodeExprLocal(ReadedLocal)), ">>", CommonShift) & (CommonMask >> CommonShift)
					, Cases
					//, new AstNodeCaseDefault(new AstNodeStmReturn(Process(default(TDecoderReference))))
				);
			}
		}
	}
}
