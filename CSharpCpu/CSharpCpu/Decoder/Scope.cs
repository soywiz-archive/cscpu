using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpCpu.Decoder
{
	public class Scope<TKey, TValue>
	{
		private Scope<TKey, TValue> Parent = null;
		private Dictionary<TKey, TValue> Items;

		public Scope(Scope<TKey, TValue> Parent = null, Dictionary<TKey, TValue> Items = null)
		{
			if (Items == null) Items = new Dictionary<TKey, TValue>();
			this.Parent = Parent;
			this.Items = Items;
		}

		public TKey[] GetAllKeys()
		{
			var Keys = (IEnumerable<TKey>)this.Items.Keys.ToArray();
			if (this.Parent != null) Keys = Keys.Concat(this.Parent.GetAllKeys()).Distinct();
			return Keys.ToArray();
		}

		//public T CreateScope<T>(Func<T> Action)
		//{
		//	var Value = default(T);
		//	CreateScope(() =>
		//	{
		//		Value = Action();
		//	});
		//	return Value;
		//}
		//
		//public void CreateScope(Action Action)
		//{
		//	var NewThis = new Scope<TKey, TValue>(this.Parent, this.Items);
		//	try
		//	{
		//		this.Parent = NewThis;
		//		this.Items = new Dictionary<TKey, TValue>();
		//		Action();
		//	}
		//	finally
		//	{
		//		this.Parent = NewThis.Parent;
		//		this.Items = NewThis.Items;
		//	}
		//}

		public bool Contains(TKey Key)
		{
			return Items.ContainsKey(Key) || ((Parent != null) && Parent.Contains(Key));
		}

		public void Set(TKey Key, TValue Value)
		{
			Items.Add(Key, Value);
		}

		public TValue GetOrCreate(TKey Key, Func<TValue> ValueGen)
		{
			if (!Contains(Key)) Set(Key, ValueGen());
			return Get(Key);
		}

		public TValue Get(TKey Key)
		{
			if (!Items.ContainsKey(Key))
			{
				if (Parent != null)
				{
					return Parent.Get(Key);
				}
				else
				{
					throw(new KeyNotFoundException("Can't find key '" + Key + "'"));
				}
			}
			else
			{
				return Items[Key];
			}
		}
	}
}
