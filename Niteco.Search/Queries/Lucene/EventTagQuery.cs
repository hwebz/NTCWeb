using System.Collections.Generic;
using EPiServer.Framework;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search.Queries.Lucene
{
    public class EventTagQuery : CollectionQueryBase
    {
        public EventTagQuery(IEnumerable<string> tags, LuceneOperator innerOperator)
            : base(EventTagField.FieldName, innerOperator)
		{
            Validator.ThrowIfNull("tags", tags);

            foreach (string num in tags)
            {
                Items.Add(num);
            }
		}
    }
}
