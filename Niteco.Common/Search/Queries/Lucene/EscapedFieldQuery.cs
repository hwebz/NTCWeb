using System;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class EscapedFieldQuery : FieldQuery
	{
		public EscapedFieldQuery(string queryExpression) : base(queryExpression)
		{
		}
		public EscapedFieldQuery(string queryExpression, Field field) : base(queryExpression, field)
		{
		}
		public override string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(base.Field));
			stringBuilder.Append(":(");
			stringBuilder.Append(LuceneHelpers.Escape(base.Expression));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
