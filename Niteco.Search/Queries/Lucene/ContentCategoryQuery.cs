using System.Globalization;
using EPiServer.Core;
using EPiServer.Framework;
using Niteco.Common.Search.Queries.Lucene;

namespace Niteco.Search.Queries.Lucene
{
    public class ContentCategoryQuery : CategoryQuery
    {
        public ContentCategoryQuery(CategoryList categories, LuceneOperator luceneOperator)
            : base(luceneOperator)
        {
            Validator.ThrowIfNull("categories", categories);

            foreach (int num in categories)
            {
                Items.Add(num.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
