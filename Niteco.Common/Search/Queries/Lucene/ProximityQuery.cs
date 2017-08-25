using System;
using System.Globalization;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class ProximityQuery : FieldQuery
	{
		public int Distance
		{
			get;
			set;
		}
		public ProximityQuery(string phrase, Field field, int distance) : base(phrase, field)
		{
			base.Expression = base.Expression;
			this.Distance = distance;
		}
		public ProximityQuery(string phrase, int distance) : base(phrase, Field.Default)
		{
			base.Expression = base.Expression;
			this.Distance = distance;
		}
		public override string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(base.Field));
			stringBuilder.Append(":(");
			stringBuilder.Append(FieldQuery.GetSafeQuotedPhrase(base.Expression));
			stringBuilder.Append("~");
			stringBuilder.Append(this.Distance.ToString(System.Globalization.CultureInfo.InvariantCulture));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
