using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
namespace Niteco.Common.Search.Queries.Lucene
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lucene", Justification = "Known name")]
	public static class LuceneHelpers
	{
		public static string Escape(string value)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (c == '\\' || c == '+' || c == '-' || c == '!' || c == '(' || c == ')' || c == ':' || c == '^' || c == '[' || c == ']' || c == '"' || c == '{' || c == '}' || c == '~' || c == '*' || c == '?' || c == '|' || c == '&')
				{
					stringBuilder.Append('\\');
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
		public static string EscapeParenthesis(string value)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				if (c == '(' || c == ')')
				{
					stringBuilder.Append('\\');
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
	}
}
