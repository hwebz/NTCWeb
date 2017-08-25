using System;
using System.Globalization;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class TermBoostQuery : FieldQuery
	{
		public float BoostFactor
		{
			get;
			set;
		}
		public TermBoostQuery(string phrase, Field field, float boostFactor) : base(phrase, field)
		{
			this.BoostFactor = boostFactor;
		}
		public TermBoostQuery(string phrase, float boostFactor) : base(phrase, Field.Default)
		{
			this.BoostFactor = boostFactor;
		}
		public override string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(base.Field));
			stringBuilder.Append(":(");
			stringBuilder.Append(FieldQuery.GetSafeQuotedPhrase(base.Expression));
			stringBuilder.Append("^");
			stringBuilder.Append(this.BoostFactor.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(",", "."));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
	}
}
