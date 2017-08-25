using System.Collections.Generic;
using EPiServer.Framework;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class CaseIndustryQuery : CollectionQueryBase
    {
        public CaseIndustryQuery(int industryId, LuceneOperator innerOperator) : base(CaseIndustryField.FieldName, innerOperator)
		{
            Validator.ThrowIfNull("industryId", industryId);

            Items.Add(industryId.ToString());
		}
    }
}
