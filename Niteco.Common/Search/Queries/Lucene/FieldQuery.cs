using System;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class FieldQuery : IQueryExpression
	{
		public Field Field
		{
			get;
			set;
		}
		public string Expression
		{
			get;
			set;
		}
		public FieldQuery(string queryExpression)
		{
			this.Field = Field.Default;
			this.Expression = queryExpression.Trim();
		}
		public FieldQuery(string queryExpression, Field field)
		{
			this.Field = field;
			this.Expression = queryExpression.Trim();
		}
		public virtual string GetQueryExpression()
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(SearchSettings.GetFieldNameForField(this.Field));
			stringBuilder.Append(":(");
			stringBuilder.Append(LuceneHelpers.EscapeParenthesis(this.Expression));
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
		protected static string GetSafeQuotedPhrase(string phrase)
		{
			if (phrase.StartsWith("\"", System.StringComparison.Ordinal))
			{
				phrase = phrase.Substring(1);
			}
			if (phrase.EndsWith("\"", System.StringComparison.Ordinal))
			{
				phrase = phrase.Substring(0, phrase.Length - 1);
			}
			return "\"" + LuceneHelpers.Escape(phrase) + "\"";
		}
	}
}
