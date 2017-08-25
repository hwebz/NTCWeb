using System.Collections.ObjectModel;

namespace Niteco.Common.Search
{
	public class SearchResults
	{
		private Collection<IndexResponseItem> _indexResponseItems = new Collection<IndexResponseItem>();
		public Collection<IndexResponseItem> IndexResponseItems
		{
			get
			{
				return this._indexResponseItems;
			}
		}
		public int TotalHits
		{
			get;
			internal set;
		}
		public string Version
		{
			get;
			internal set;
		}
	}
}
