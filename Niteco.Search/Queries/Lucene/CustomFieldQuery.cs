using System.Globalization;
using Niteco.Common.Search.Queries;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class CustomFieldQuery : IQueryExpression
    {
        public CustomFieldQuery(string queryExpression, CustomField customField)
        {
            Expression = queryExpression;
            Field = customField;
        }

        //public CustomFieldQuery(string queryExpression, string fieldName)
        //{
        //    Expression = queryExpression;
        //    Field = fieldName;
        //    Boost = null;
        //}

        //public CustomFieldQuery(string queryExpression, string fieldName, float boost)
        //{
        //    Expression = queryExpression;
        //    Field = fieldName;
        //    Boost = boost;
        //}

        public string GetQueryExpression()
        {
            if (this.Field == null)
            {
                return string.Empty;
            }

            return string.Format("{0}:({1}{2})", this.Field.Name, LuceneHelpers.EscapeParenthesis(Expression),
                this.Field.Boost.HasValue
                    ? string.Concat("^", this.Field.Boost.Value.ToString(CultureInfo.InvariantCulture).Replace(",", "."))
                    : string.Empty);
        }

        ///public string Field { get; set; }

        public CustomField Field { get; set; }

        public string Expression { get; set; }

        //public float? Boost { get; set; }
    }
}
