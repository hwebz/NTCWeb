using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Niteco.Common.Search.Queries.Lucene
{
	public class GroupQuery : IQueryExpression
	{
		private Collection<IQueryExpression> _queries = new Collection<IQueryExpression>();
		public LuceneOperator InnerOperator
		{
			get;
			set;
		}
		public Collection<IQueryExpression> QueryExpressions
		{
			get
			{
				return this._queries;
			}
		}
		public GroupQuery(LuceneOperator innerOperator)
		{
			this.InnerOperator = innerOperator;
		}
		public string GetQueryExpression()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			int count = this.QueryExpressions.Count;
			foreach (IQueryExpression current in this.QueryExpressions)
			{
				num++;
				if (count > 1)
				{
					stringBuilder.Append("(");
				}
				stringBuilder.Append(current.GetQueryExpression());
				if (num < this.QueryExpressions.Count)
				{
					stringBuilder.Append(") " + Enum.GetName(typeof(LuceneOperator), this.InnerOperator) + " ");
				}
				else
				{
					if (count > 1)
					{
						stringBuilder.Append(")");
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
