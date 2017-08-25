using System;
using System.Configuration.Provider;
namespace Niteco.Common.Search.Filter
{
	public abstract class SearchResultFilterProvider : ProviderBase
	{
		public abstract SearchResultFilter Filter(IndexResponseItem item);
	}
}
