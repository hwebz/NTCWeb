namespace Niteco.Search.IndexingService
{
	public class OptimizedEventArgs : System.EventArgs
	{
		public string NamedIndex
		{
			get;
			set;
		}
		public OptimizedEventArgs(string namedIndex)
		{
			this.NamedIndex = namedIndex;
		}
	}
}
