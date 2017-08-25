using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using log4net;
using Niteco.Common.Search.Configuration;

namespace Niteco.Common.Search
{
	[InitializableModule]
	public class SearchInitialization : IInitializableModule
	{
		public void Initialize(InitializationEngine context)
		{
			if (SearchSettings.Config == null)
			{
				return;
			}
			SearchSettings.Log = LogManager.GetLogger(typeof(SearchSettings).Name);
			SearchSettings.LoadSearchResultFilterProviders(SearchSettings.Config.SearchResultFilterElement);
			SearchSettings.LoadNamedIndexingServices(SearchSettings.Config.NamedIndexingServices);
			foreach (NamedIndexingServiceElement current in SearchSettings.IndexingServices.Values)
			{
				current.GetClientCertificate();
			}
			if (context == null || context.HostType != HostType.Installer)
			{
				RequestQueueHandler.StartQueueFlushTimer();
			}
			else
			{
				SearchSettings.Log.Info("Didn't start the Queue Flush timer, since HostType is 'Installer'");
			}
			SearchSettings.OnInitializationCompleted();
			SearchSettings.Log.Info("Search Module Started!");
		}
		public void Uninitialize(InitializationEngine context)
		{
			SearchSettings.IndexingServices.Clear();
			SearchSettings.SearchResultFilterProviders.Clear();
			SearchSettings.Log.Info("Search Module Stopped!");
		}
		public void Preload(string[] parameters)
		{
		}
	}
}
