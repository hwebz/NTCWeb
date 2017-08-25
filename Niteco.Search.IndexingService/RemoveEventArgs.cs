namespace Niteco.Search.IndexingService
{
	public class RemoveEventArgs : System.EventArgs
	{
		public string DocumentId
		{
			get;
			set;
		}
		public string NamedIndex
		{
			get;
			set;
		}
		public RemoveEventArgs(string documentId, string namedIndex)
		{
			this.DocumentId = documentId;
			this.NamedIndex = namedIndex;
		}
	}
}
