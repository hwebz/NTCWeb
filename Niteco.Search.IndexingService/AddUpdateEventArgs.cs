using Lucene.Net.Documents;

namespace Niteco.Search.IndexingService
{
	public class AddUpdateEventArgs : System.EventArgs
	{
		public Document Document
		{
			get;
			set;
		}
		public string NamedIndex
		{
			get;
			set;
		}
		public AddUpdateEventArgs(Document document, string namedIndex)
		{
			this.Document = document;
			this.NamedIndex = namedIndex;
		}
	}
}
