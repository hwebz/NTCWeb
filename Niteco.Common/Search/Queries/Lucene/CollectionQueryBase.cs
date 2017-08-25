using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public abstract class CollectionQueryBase : IQueryExpression
	{
		private System.Collections.ObjectModel.Collection<string> _items = new System.Collections.ObjectModel.Collection<string>();
		public LuceneOperator InnerOperator
		{
			get;
			private set;
		}
		public string IndexFieldName
		{
			get;
			private set;
		}
		public System.Collections.ObjectModel.Collection<string> Items
		{
			get
			{
				return this._items;
			}
		}
		protected CollectionQueryBase(string itemFieldName, LuceneOperator innerOperator)
		{
			this.InnerOperator = innerOperator;
			this.IndexFieldName = itemFieldName;
		}
		public virtual string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			System.Collections.ObjectModel.Collection<string> collection = CollectionQueryBase.RemoveDuplicates(this.Items);
			int num = 0;
			foreach (string current in collection)
			{
				num++;
				stringBuilder.Append(this.IndexFieldName + ":(");
				if (num < collection.Count)
				{
					stringBuilder.Append(LuceneHelpers.Escape(current));
					stringBuilder.Append(") ");
					stringBuilder.Append(System.Enum.GetName(typeof(LuceneOperator), this.InnerOperator));
					stringBuilder.Append(" ");
				}
				else
				{
					stringBuilder.Append(LuceneHelpers.Escape(current));
					stringBuilder.Append(")");
				}
			}
			return stringBuilder.ToString();
		}
		private static System.Collections.ObjectModel.Collection<string> RemoveDuplicates(System.Collections.ObjectModel.Collection<string> inputList)
		{
			System.Collections.Generic.Dictionary<string, int> dictionary = new System.Collections.Generic.Dictionary<string, int>();
			System.Collections.ObjectModel.Collection<string> collection = new System.Collections.ObjectModel.Collection<string>();
			foreach (string current in inputList)
			{
				if (!dictionary.ContainsKey(current))
				{
					dictionary.Add(current, 0);
					collection.Add(current);
				}
			}
			return collection;
		}
	}
}
