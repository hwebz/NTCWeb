using EPiServer.Core;
using Niteco.Common.Search.Queries;
using Niteco.Common.Search.Queries.Lucene;


namespace Niteco.Search.Queries.Lucene
{
    public class ContentQuery<T> : IQueryExpression where T : IContentData
    {
        public virtual string GetQueryExpression()
        {
            return new FieldQuery("\"" + LuceneContentSearchHandler.GetItemTypeSection<T>() + "\"", Field.ItemType).GetQueryExpression();
        }
    }
}
