using EPiServer.ServiceLocation;
using System;
namespace Niteco.Common.Search
{
	public static class ServiceLocationHelperExtensions
	{
		public static SearchHandler SearchHandler(this ServiceLocationHelper helper)
		{
			return helper.Advanced.GetInstance<SearchHandler>();
		}
	}
}
