using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.ServiceLocation;
using log4net;

namespace Niteco.Common.Search
{
	[ServiceConfiguration(Lifecycle = ServiceInstanceScope.Singleton)]
	public class ReIndexManager
	{
		private static ILog _log = LogManager.GetLogger(typeof(ReIndexManager));
		private IList<IReIndexable> _reindexableServices;
		public IList<IReIndexable> ReIndexableServices
		{
			get
			{
				return this._reindexableServices ?? ServiceLocator.Current.GetAllInstances<IReIndexable>().ToList<IReIndexable>();
			}
			internal set
			{
				this._reindexableServices = value;
			}
		}
		public void ReIndex()
		{
			foreach (IReIndexable current in this.ReIndexableServices)
			{
				try
				{
					if (_log.IsInfoEnabled)
					{
						_log.Info(string.Format(CultureInfo.InvariantCulture, "Start Reset index of the type: {0}, Startup Time: {1}", new object[]
						{
							current.GetType(),
							DateTime.Now
						}));
					}
					SearchHandler.Instance.ResetIndex(current.NamedIndexingService, current.NamedIndex);
					if (_log.IsInfoEnabled)
					{
						_log.Info(string.Format(CultureInfo.InvariantCulture, "Finish Reset  of the type: {0}, Finished Time: {1}", new object[]
						{
							current.GetType(),
							DateTime.Now
						}));
					}
				}
				catch (Exception exception)
				{
					if (_log.IsErrorEnabled)
					{
						_log.Error(string.Format(CultureInfo.InvariantCulture, "Failed to reset of the service type: {0}", new object[]
						{
							current.GetType()
						}), exception);
					}
				}
			}
			foreach (IReIndexable current2 in this.ReIndexableServices)
			{
				try
				{
					if (_log.IsInfoEnabled)
					{
						_log.Info(string.Format(CultureInfo.InvariantCulture, "Start reindexing of the type: {0}, Startup Time: {1}", new object[]
						{
							current2.GetType(),
							DateTime.Now
						}));
					}
					current2.ReIndex();
					if (_log.IsInfoEnabled)
					{
						_log.Info(string.Format(CultureInfo.InvariantCulture, "Finish reindexing of the type: {0}, Finished Time: {1}", new object[]
						{
							current2.GetType(),
							DateTime.Now
						}));
					}
				}
				catch (Exception exception2)
				{
					if (_log.IsErrorEnabled)
					{
						_log.Error(string.Format(CultureInfo.InvariantCulture, "Failed to reindex of the service type: {0}", new object[]
						{
							current2.GetType()
						}), exception2);
					}
				}
			}
		}
	}
}
