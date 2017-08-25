using System;
namespace Niteco.Common.Search.Filter
{
	public sealed class SearchResultFilterHandler
	{
		private SearchResultFilterHandler()
		{
		}
		public static bool Include(IndexResponseItem item)
		{
			foreach (SearchResultFilterProvider current in SearchSettings.SearchResultFilterProviders.Values)
			{
				SearchResultFilter searchResultFilter = current.Filter(item);
				if (searchResultFilter == SearchResultFilter.Include)
				{
					bool result = true;
					return result;
				}
				if (searchResultFilter == SearchResultFilter.Exclude)
				{
					bool result = false;
					return result;
				}
			}
			return SearchSettings.Config.SearchResultFilterElement.SearchResultFilterDefaultInclude;
		}
	}
}
