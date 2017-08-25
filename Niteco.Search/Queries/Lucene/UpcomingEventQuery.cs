using System;
using System.Text;
using System.Text.RegularExpressions;
using Niteco.Common.Search.Queries;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class UpcomingEventQuery : IQueryExpression
    {
        public bool Inclusive { get; protected set; }

        public UpcomingEventQuery(bool inclusive)
        {
            this.Inclusive = inclusive;
        }

        public string GetQueryExpression()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(EventDateField.FieldName);
            stringBuilder.Append(":");
            stringBuilder.Append(this.Inclusive ? "[" : "{");
            stringBuilder.Append(LuceneHelpers.Escape(Regex.Replace(DateTime.Now.Date.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", "")));
            stringBuilder.Append(" TO ");
            stringBuilder.Append(LuceneHelpers.Escape(Regex.Replace(DateTime.MaxValue.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", "")));
            stringBuilder.Append(this.Inclusive ? "]" : "}");
            return stringBuilder.ToString();
        }
    }
}
