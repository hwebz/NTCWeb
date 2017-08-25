using EPiServer.Framework;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class ProductQuery : CollectionQueryBase
    {
        public ProductQuery(int productId, LuceneOperator innerOperator)
            : base(ProductField.FieldName, innerOperator)
		{
            Validator.ThrowIfNull("productId", productId);

            Items.Add(productId.ToString());
		}
    }
}
