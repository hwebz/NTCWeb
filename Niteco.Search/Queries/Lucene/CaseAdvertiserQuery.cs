using System.Collections.Generic;
using EPiServer.Framework;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class CaseAdvertiserQuery : CollectionQueryBase
    {
        public CaseAdvertiserQuery(int advertiserId, LuceneOperator innerOperator) : base(CaseAdvertiserField.FieldName, innerOperator)
		{
            Validator.ThrowIfNull("advertiserId", advertiserId);

            Items.Add(advertiserId.ToString());
		}
    }
}
