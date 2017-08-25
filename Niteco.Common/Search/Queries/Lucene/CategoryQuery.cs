using System;
namespace Niteco.Common.Search.Queries.Lucene
{
	public class CategoryQuery : CollectionQueryBase
	{
		public CategoryQuery(LuceneOperator innerOperator) : base(SearchSettings.Config.IndexingServiceFieldNameCategories, innerOperator)
		{
		}
	}
}
