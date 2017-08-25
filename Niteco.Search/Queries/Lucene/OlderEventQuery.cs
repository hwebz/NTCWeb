using System;
using System.Text;
using System.Text.RegularExpressions;
using Niteco.Common.Search.Queries;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class OlderEventQuery : IQueryExpression
    {
        public string GetQueryExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(EventDateField.FieldName);
            stringBuilder.Append(":");
            stringBuilder.Append("{");
            stringBuilder.Append(LuceneHelpers.Escape(Regex.Replace(DateTime.MinValue.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", "")));
            stringBuilder.Append(" TO ");
            stringBuilder.Append(LuceneHelpers.Escape(Regex.Replace(DateTime.Now.Date.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", "")));
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }
    }
}
