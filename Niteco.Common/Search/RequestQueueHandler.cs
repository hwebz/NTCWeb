using EPiServer.Data;
using Niteco.Common.Search.Data;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Timers;
namespace Niteco.Common.Search
{
	public sealed class RequestQueueHandler
	{
		private static Timer _queueFlushTimer;
		private static object _syncObject;
		public static event System.EventHandler QueueProcessed;
		private RequestQueueHandler()
		{
		}
		static RequestQueueHandler()
		{
			RequestQueueHandler._syncObject = new object();
			RequestQueueHandler._queueFlushTimer = new Timer((double)(SearchSettings.Config.QueueFlushInterval * 1000));
			RequestQueueHandler._queueFlushTimer.AutoReset = false;
			RequestQueueHandler._queueFlushTimer.Elapsed += new ElapsedEventHandler(RequestQueueHandler.Timer_Elapsed);
		}
		internal static void Enqueue(IndexRequestItem request, string namedIndexingService)
		{
			SearchFactory.AddToQueue(request, namedIndexingService);
		}
		public static void TruncateQueue()
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Search is not set as active in configuration");
			}
			SearchFactory.TruncateQueue();
		}
		public static void TruncateQueue(string namedIndexingService, string namedIndex)
		{
			if (!SearchSettings.Config.Active)
			{
				throw new System.InvalidOperationException("Can not perform this operation when Niteco.Search is not set as active in configuration");
			}
			SearchFactory.TruncateQueue(namedIndexingService, namedIndex);
		}
		internal static void StartQueueFlushTimer()
		{
			RequestQueueHandler._queueFlushTimer.Enabled = true;
		}
		public static void ProcessQueue()
		{
			lock (RequestQueueHandler._syncObject)
			{
				int dequeuePageSize = SearchSettings.Config.DequeuePageSize;
				SearchSettings.Log.Debug(string.Format("Start dequeue unprocessed items", new object[0]));
				foreach (string current in SearchSettings.IndexingServices.Keys)
				{
					while (true)
					{
						try
						{
							System.Collections.ObjectModel.Collection<Identity> ids;
							SyndicationFeed unprocessedQueueItems = SearchFactory.GetUnprocessedQueueItems(current, dequeuePageSize, out ids);
							if (unprocessedQueueItems.Items.Count<SyndicationItem>() >= 1)
							{
								SearchSettings.Log.Debug(string.Format("Start processing batch for indexing service '{0}'", current));
								if (RequestHandler.Instance.SendRequest(unprocessedQueueItems, current, ids))
								{
									SearchFactory.RemoveProcessedQueueItems(ids);
									SearchSettings.Log.Debug(string.Format("End processing batch", new object[0]));
									continue;
								}
								SearchSettings.Log.Error(string.Format("Send batch for named index '{0}' failed. Items are left in queue.", current));
							}
						}
						catch (System.Exception ex)
						{
							SearchSettings.Log.Error(string.Format("RequestQueue failed to retrieve unprocessed queue items. Message: {0}{1}Stacktrace: {2}", ex.Message, System.Environment.NewLine, ex.StackTrace));
						}
						break;
					}
				}
				SearchSettings.Log.Debug(string.Format("End dequeue unprocessed items", new object[0]));
				RequestQueueHandler.OnQueueProcessed();
			}
		}
		private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				RequestQueueHandler.ProcessQueue();
			}
			finally
			{
				RequestQueueHandler._queueFlushTimer.Enabled = true;
			}
		}
		internal static void OnQueueProcessed()
		{
			if (RequestQueueHandler.QueueProcessed != null)
			{
				RequestQueueHandler.QueueProcessed(null, new System.EventArgs());
			}
		}
	}
}
