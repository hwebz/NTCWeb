using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Niteco.Search.IndexingService.Configuration;

namespace Niteco.Search.IndexingService.Security
{
	public class SecurityHandler
	{
		private static SecurityHandler _instance;
		public static SecurityHandler Instance
		{
			get
			{
				return SecurityHandler._instance;
			}
			set
			{
				SecurityHandler._instance = value;
			}
		}
		static SecurityHandler()
		{
			SecurityHandler._instance = new SecurityHandler();
		}
		protected SecurityHandler()
		{
		}
		protected internal virtual bool IsAuthenticated(string accessKey, AccessLevel accessLevel)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Request for authorization for access key '{0}'", accessKey));
			if (string.IsNullOrEmpty(accessKey))
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error("No access key found. Access denied.");
				return false;
			}
			ClientElement clientElement;
			if (!IndexingServiceSettings.ClientElements.TryGetValue(accessKey, out clientElement))
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("The access key: '{0}' was not found for configured clients. Access denied.", accessKey));
				return false;
			}
			if (clientElement.ReadOnly && accessLevel == AccessLevel.Modify)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("Modify request for access key '{0}' failed. Only read access", accessKey));
				return false;
			}
			OperationContext current = OperationContext.Current;
			if (current != null)
			{
				MessageProperties incomingMessageProperties = current.IncomingMessageProperties;
				RemoteEndpointMessageProperty remoteEndpointMessageProperty = incomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
				if (!clientElement.IsIPAddressAllowed(IPAddress.Parse(remoteEndpointMessageProperty.Address)))
				{
					IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("No match for client IP {0}. Access denied for access key {1}.", remoteEndpointMessageProperty.Address, accessKey));
					return false;
				}
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Request for authorization for access key '{0}' succeded", accessKey));
			return true;
		}
	}
}
