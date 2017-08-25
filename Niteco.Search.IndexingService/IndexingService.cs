using System.ServiceModel.Activation;
using System.ServiceModel.Syndication;
using Niteco.Search.IndexingService.Security;

namespace Niteco.Search.IndexingService
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class IndexingService : IIndexingService
	{
		public static event System.EventHandler DocumentAdding;
		public static event System.EventHandler DocumentAdded;
		public static event System.EventHandler DocumentRemoving;
		public static event System.EventHandler DocumentRemoved;
		public static event System.EventHandler IndexOptimized;
		public static event System.EventHandler InternalServerError;
		public virtual void UpdateIndex(string accessKey, SyndicationFeedFormatter formatter)
		{
			if (!SecurityHandler.Instance.IsAuthenticated(accessKey, AccessLevel.Modify))
			{
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return;
			}
			IndexingServiceHandler.Instance.UpdateIndex(formatter.Feed);
		}
		public virtual SyndicationFeedFormatter GetSearchResultsXml(string q, string namedIndexes, string offset, string limit, string accessKey, string sort)
		{
			if (!SecurityHandler.Instance.IsAuthenticated(accessKey, AccessLevel.Read))
			{
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return null;
			}
			return this.GetSearchResults(q, namedIndexes, int.Parse(offset, System.Globalization.CultureInfo.InvariantCulture), int.Parse(limit, System.Globalization.CultureInfo.InvariantCulture), sort);
		}
        public SyndicationFeedFormatter GetSearchResultsJson(string q, string namedIndexes, string offset, string limit, string accessKey, string sort)
		{
			if (!SecurityHandler.Instance.IsAuthenticated(accessKey, AccessLevel.Read))
			{
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return null;
			}
			return this.GetSearchResults(q, namedIndexes, int.Parse(offset, System.Globalization.CultureInfo.InvariantCulture), int.Parse(limit, System.Globalization.CultureInfo.InvariantCulture), sort);
		}
		public void ResetIndex(string namedIndex, string accessKey)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Reset of index: {0} requested", namedIndex));
			if (!SecurityHandler.Instance.IsAuthenticated(accessKey, AccessLevel.Modify))
			{
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return;
			}
			IndexingServiceHandler.Instance.ResetNamedIndex(namedIndex);
		}
		public SyndicationFeedFormatter GetNamedIndexes(string accessKey)
		{
			if (!SecurityHandler.Instance.IsAuthenticated(accessKey, AccessLevel.Read))
			{
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return null;
			}
			return IndexingServiceHandler.Instance.GetNamedIndexes();
		}

        #region Customzied
        // Hieu Le: Added sort param
        private SyndicationFeedFormatter GetSearchResults(string q, string namedIndexes, int offset, int limit, string sort)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Request for search with query parser with expression: {0} in named indexes: {1}", q, namedIndexes));
			string[] namedIndexNames = null;
			if (!string.IsNullOrEmpty(namedIndexes))
			{
				char[] separator = new char[]
				{
					'|'
				};
				namedIndexNames = namedIndexes.Split(separator);
			}

		    string[] sortFields = null;
            if (!string.IsNullOrEmpty(sort))
		    {
                sortFields = sort.Split(new[] { '|' });
		    }

            return IndexingServiceHandler.Instance.GetSearchResults(q, namedIndexNames, offset, limit, sortFields);
		}
        #endregion
        internal static void OnDocumentAdding(object sender, AddUpdateEventArgs e)
		{
			if (IndexingService.DocumentAdding != null)
			{
				IndexingService.DocumentAdding(sender, e);
			}
		}
		internal static void OnDocumentAdded(object sender, AddUpdateEventArgs e)
		{
			if (IndexingService.DocumentAdded != null)
			{
				IndexingService.DocumentAdded(sender, e);
			}
		}
		internal static void OnDocumentRemoving(object sender, RemoveEventArgs e)
		{
			if (IndexingService.DocumentRemoving != null)
			{
				IndexingService.DocumentRemoving(sender, e);
			}
		}
		internal static void OnDocumentRemoved(object sender, RemoveEventArgs e)
		{
			if (IndexingService.DocumentRemoved != null)
			{
				IndexingService.DocumentRemoved(sender, e);
			}
		}
		internal static void OnIndexedOptimized(object sender, OptimizedEventArgs e)
		{
			if (IndexingService.IndexOptimized != null)
			{
				IndexingService.IndexOptimized(sender, e);
			}
		}
		internal static void OnInternalServerError(object sender, InternalServerErrorEventArgs e)
		{
			if (IndexingService.InternalServerError != null)
			{
				IndexingService.InternalServerError(sender, e);
			}
		}
	}
}
