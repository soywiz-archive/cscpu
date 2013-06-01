using SafeILGenerator.Ast.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Decoder
{
	public class SwitchGenerator
	{
		private AstNodeExpr Process(IDecoderReference DecoderReference)
		{
			return null;
		}

		public AstNodeStm GenerateSwitch(IEnumerable<IDecoderReference> Items, uint BaseMask = 0xFFFFFFFF, int ReferenceIndex = 0)
		{
			Items = Items.Where(Item => Item.Mask.Length >= ReferenceIndex);

			var CommonMask = Items
				.Aggregate(BaseMask, (CurrentMask, Item) => CurrentMask & Item.Mask[ReferenceIndex])
			;

			
			return new AstNodeStmSwitch(
				(new AstNodeExprImm(0)) & CommonMask,
				Items.Select((Item) => new AstNodeCase(Item.Data[ReferenceIndex] & CommonMask, new AstNodeStmReturn(Process(Item)))).ToArray()
			);
		}
	}
}
