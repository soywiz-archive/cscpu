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
		static private AstGenerator ast = AstGenerator.Instance;

		public class DecoderContext<TDecoderReference>
		{
			public TDecoderReference DecoderReference;
			public Scope<string, AstLocal> Scope;

			internal DecoderContext()
			{
			}
		}

		static public AstNodeStm GenerateSwitchNoReturnValue<TDecoderReference>(IEnumerable<TDecoderReference> AllItems, Func<DecoderContext<TDecoderReference>, AstNodeStm> Process, uint BaseMask = 0xFFFFFFFF) where TDecoderReference : IDecoderReference
		{
			return ast.Statements(
				new SwitchGeneratorInternal<TDecoderReference>()
				{
					ReaderArgument = new AstArgument(0, typeof(SwitchReadWordDelegate), "Read"),
					AllItems = AllItems,
					Process = Process,
					BaseMask = BaseMask,
				}.GenerateSwitch(AllItems, 0),
				ast.Return()
			);
		}

		static public AstNodeStm GenerateSwitchReturnValue<TReturnType, TDecoderReference>(IEnumerable<TDecoderReference> AllItems, Func<DecoderContext<TDecoderReference>, AstNodeStm> Process, uint BaseMask = 0xFFFFFFFF) where TDecoderReference : IDecoderReference
		{
			return ast.Statements(
				new SwitchGeneratorInternal<TDecoderReference>() {
					ReaderArgument = new AstArgument(0, typeof(SwitchReadWordDelegate), "Read"),
					AllItems = AllItems,
					Process = Process,
					BaseMask = BaseMask,
				}.GenerateSwitch(AllItems, 0),

				ast.Return(ast.Immediate(default(TReturnType)))
			);
		}

		sealed internal class SwitchGeneratorInternal<TDecoderReference> where TDecoderReference : IDecoderReference
		{
			internal AstArgument ReaderArgument;
			internal Func<DecoderContext<TDecoderReference>, AstNodeStm> Process;
			internal IEnumerable<TDecoderReference> AllItems;
			internal uint BaseMask;
			internal Scope<string, AstLocal> LocalScope = new Scope<string, AstLocal>();

			internal AstNodeStm GenerateSwitch(IEnumerable<TDecoderReference> Items, int ReferenceIndex)
			{
				var NewLocalScope = new Scope<string, AstLocal>(LocalScope);
				var OldLocalScope = LocalScope;
				LocalScope = NewLocalScope;
				try {
					LocalScope.GetOrCreate("Word" + ReferenceIndex, () => { return AstLocal.Create(typeof(uint), "Word" + ReferenceIndex); });
					var Statements = ast.Statements();
					{
						Statements.AddStatement(ast.Assign(ast.Local(LocalScope.Get("Word" + ReferenceIndex)), ast.CallDelegate(ast.Argument(ReaderArgument))));
						Statements.AddStatement(_GenerateSwitch(this.BaseMask, Items, ReferenceIndex, 0));
						Statements.AddStatement(_GenerateLeaf(default(TDecoderReference), ReferenceIndex));
					}
					return Statements;
				} finally {
					LocalScope = OldLocalScope;
				}
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

				var Stats = ast.Statements();

				if (DecoderReference != null && DecoderReference.MaskDataVars != null)
				{
					if (ReferenceIndex < DecoderReference.MaskDataVars.Length)
					{
						for (int RI = 0; RI <= ReferenceIndex; RI++)
						{
							if (DecoderReference.MaskDataVars[RI].Vars.Length != 0)
							{
								foreach (var Var in DecoderReference.MaskDataVars[RI].Vars)
								{
									//Console.Write("{0},", Var);
									var Local = LocalScope.GetOrCreate(Var.Name, () => {
										return AstLocal.Create(typeof(uint), Var.Name);
									});
									Stats.AddStatement(ast.Assign(
										ast.Local(Local),
										ast.Binary(ast.Binary(ast.Local(LocalScope.Get("Word" + RI)), ">>", Var.Shift), "&", Var.Mask)
									));
								}
							}
						}
						//Console.WriteLine("");
					}
				}

				Stats.AddStatement(Process(
					new DecoderContext<TDecoderReference>()
					{
						DecoderReference = DecoderReference,
						Scope = LocalScope,
					}
				));

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
					if (Items.Count() == 1 && ReferenceIndex >= Items.First().MaskDataVars.Length - 1)
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

					Cases.Add(ast.Case(((Group.First().MaskDataVars[ReferenceIndex].Data >> CommonShift) & (CommonMask >> CommonShift)), CaseBody));
				}

				return ast.Switch(
					ast.Binary((ast.Local(LocalScope.Get("Word" + ReferenceIndex))), ">>", CommonShift) & (CommonMask >> CommonShift),
					Cases.ToArray()
				);
			}
		}
	}
}
