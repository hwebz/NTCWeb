using System;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class RangeQuery : IQueryExpression
	{
		public string Start
		{
			get;
			set;
		}
		public string End
		{
			get;
			set;
		}
		public Field Field
		{
			get;
			set;
		}
		public bool Inclusive
		{
			get;
			set;
		}
		public RangeQuery(string start, string end, Field field, bool inclusive)
		{
			this.Start = start;
			this.End = end;
			this.Field = field;
			this.Inclusive = inclusive;
		}
		public string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(this.Field));
			stringBuilder.Append(":");
			stringBuilder.Append(this.Inclusive ? "[" : "{");
			stringBuilder.Append(LuceneHelpers.Escape(this.Start));
			stringBuilder.Append(" TO ");
			stringBuilder.Append(LuceneHelpers.Escape(this.End));
			stringBuilder.Append(this.Inclusive ? "]" : "}");
			return stringBuilder.ToString();
		}
	}
}
