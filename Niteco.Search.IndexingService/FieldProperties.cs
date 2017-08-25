using Lucene.Net.Documents;

namespace Niteco.Search.IndexingService
{
	internal class FieldProperties
	{
		internal Field.Store FieldStore
		{
			get;
			set;
		}
		internal Field.Index FieldIndex
		{
			get;
			set;
		}
	}
}
