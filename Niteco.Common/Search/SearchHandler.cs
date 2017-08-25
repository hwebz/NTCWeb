using System.Collections.ObjectModel;
using Niteco.Common.Search.Queries;
using EPiServer.ServiceLocation;

namespace Niteco.Common.Search
{
	[ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton, FactoryMember = "Instance")]
	public class SearchHandler
	{
        private static SearchHandler instance;

        public static SearchHandler Instance
        {
            get
            {
                return SearchHandler.instance;
            }
            set
            {
                SearchHandler.instance = value;
            }
        }

        protected SearchHandler()
        {
        }

        static SearchHandler()
        {
            SearchHandler.instance = new SearchHandler();
        }

		public virtual void UpdateIndex(IndexRequestItem item)
		{
			this.UpdateIndex(item, null);
		}

		public virtual void UpdateIndex(IndexRequestItem item, string namedIndexingService)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Common.Search is not set as active in configuration");
			}

			if (item == null)
			{
				throw new System.ArgumentNullException("item");
			}

			if (string.IsNullOrEmpty(item.Id))
			{
				throw new System.ArgumentException("The Id property cannot be null");
			}

			RequestQueueHandler.Enqueue(item, namedIndexingService);
		}

        public virtual SearchResults GetSearchResults(IQueryExpression queryExpression, int page, int pageSize, Collection<string> sortFields)
		{
            return this.GetSearchResults(queryExpression, null, null, page, pageSize, sortFields);
		}

		public virtual SearchResults GetSearchResults(IQueryExpression queryExpression, string namedIndexingService, Collection<string> namedIndexes, int page, int pageSize, Collection<string> sortFields)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Common.Search is not set as active in configuration");
			}
			if (page <= 0)
			{
				throw new System.ArgumentOutOfRangeException("page", page, "The search result page cannot be less than 1");
			}
			if (pageSize < 0)
			{
				throw new System.ArgumentOutOfRangeException("pageSize", page, "The number of results returned can not be less than 0");
			}
			int offset = page * pageSize - pageSize;
			return RequestHandler.Instance.GetSearchResults(queryExpression.GetQueryExpression(), namedIndexingService, namedIndexes, offset, pageSize, sortFields);
		}

		public virtual Collection<string> GetNamedIndexes(string namedIndexingService)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Common.Search is not set as active in configuration");
			}
			return RequestHandler.Instance.GetNamedIndexes(namedIndexingService);
		}

		public virtual Collection<string> GetNamedIndexes()
		{
			return this.GetNamedIndexes(null);
		}

		public virtual void ResetIndex(string namedIndexingService, string namedIndex)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Common.Search is not set as active in configuration");
			}
			RequestHandler.Instance.ResetIndex(namedIndexingService, namedIndex);
		}

		public virtual void ResetIndex(string namedIndex)
		{
			this.ResetIndex(null, namedIndex);
		}

		public virtual void TruncateQueue(string namedIndexingService, string namedIndex)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Common.Search is not set as active in configuration");
			}
			RequestQueueHandler.TruncateQueue(namedIndexingService, namedIndex);
		}

		internal void ResetAllIndex()
		{
			if (!SearchSettings.Config.Active)
			{
				return;
			}
			foreach (string current in this.GetNamedIndexes())
			{
				this.ResetIndex(current);
			}
		}
	}
}
