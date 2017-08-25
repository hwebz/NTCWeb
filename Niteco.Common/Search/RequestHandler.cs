using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Syndication;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using EPiServer.Data;
using Niteco.Common.Search.Configuration;
using Niteco.Common.Search.Filter;

namespace Niteco.Common.Search
{
	public class RequestHandler
	{
		private static RequestHandler _instance;
		public static event EventHandler UpdateRequestSent;
		public static event EventHandler UpdateRequestSending;
		public static event EventHandler ResetRequestSent;
		public static event EventHandler ResetRequestSending;
		public static event EventHandler SearchRequestSending;
		public static event EventHandler SearchRequestSent;
		public static RequestHandler Instance
		{
			get
			{
				return _instance;
			}
			set
			{
				_instance = value;
			}
		}
		protected RequestHandler()
		{
		}
		static RequestHandler()
		{
			_instance = new RequestHandler();
		}
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        protected internal virtual bool SendRequest(SyndicationFeed feed, string namedIndexingService, Collection<Identity> ids)
        {
            NamedIndexingServiceElement namedIndexingServiceElement = SearchSettings.IndexingServices[namedIndexingService];
            bool result;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    CheckCharacters = false,
                    CloseOutput = false
                }))
                {
                    feed.GetAtom10Formatter().WriteTo(xmlWriter);
                    xmlWriter.Flush();
                }
                memoryStream.Position = 0L;
                string text = SearchSettings.Config.UpdateUriTemplate.Replace("{accesskey}", namedIndexingServiceElement.AccessKey);
                text = namedIndexingServiceElement.BaseUri + text;
                OnUpdateRequestSending(this, new EventArgs());
                try
                {
                    MakeHttpRequest(text, namedIndexingServiceElement, "POST", memoryStream, null);
                    OnUpdateRequestSent(this, new EventArgs());
                    result = true;
                    return result;
                }
                catch (Exception ex)
                {
                    SearchSettings.Log.Error(string.Format("Update batch could not be sent to service uri '{0}'. Message: '{1}{2}'", text, ex.Message, ex.StackTrace));
                }
                result = false;
            }
            return result;
        }
        
		protected internal virtual void ResetIndex(string namedIndexingService, string namedIndex)
		{
			NamedIndexingServiceElement namedIndexingServiceElement = SearchSettings.GetNamedIndexingServiceElement(namedIndexingService);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("{namedindex}", namedIndex);
			dictionary.Add("{accesskey}", namedIndexingServiceElement.AccessKey);
			string text = SearchSettings.Config.ResetUriTemplate;
			foreach (string current in dictionary.Keys)
			{
				text = text.Replace(current, HttpUtility.UrlEncode(dictionary[current]));
			}
			text = namedIndexingServiceElement.BaseUri + text;
			OnResetRequestSending(this, new EventArgs());
			try
			{
				MakeHttpRequest(text, namedIndexingServiceElement, SearchSettings.Config.ResetHttpMethod, null, null);
				OnResetRequestSent(this, new EventArgs());
			}
			catch (Exception ex)
			{
				SearchSettings.Log.Error(string.Format("Could not reset index '{0}' for service uri '{1}'. Message: {2}{3}", new object[]
				{
					namedIndex,
					text,
					ex.Message,
					ex.StackTrace
				}));
			}
		}
		protected internal virtual Collection<string> GetNamedIndexes(string namedIndexingService)
		{
			NamedIndexingServiceElement namedIndexingServiceElement = SearchSettings.GetNamedIndexingServiceElement(namedIndexingService);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("{accesskey}", namedIndexingServiceElement.AccessKey);
			string text = SearchSettings.Config.NamedIndexesUriTemplate;
			foreach (string current in dictionary.Keys)
			{
				text = text.Replace(current, HttpUtility.UrlEncode(dictionary[current]));
			}
			text = namedIndexingServiceElement.BaseUri + text;
			XmlReader xmlReader = null;
			SyndicationFeed feed = null;
			Collection<string> namedIndexes = new Collection<string>();
			try
			{
				MakeHttpRequest(text, namedIndexingServiceElement, "GET", null, delegate(WebResponse response)
				{
					xmlReader = XmlReader.Create(response.GetResponseStream());
					feed = SyndicationFeed.Load(xmlReader);
					foreach (SyndicationItem current2 in feed.Items)
					{
						namedIndexes.Add(current2.Title.Text);
					}
				});
			}
			catch (Exception ex)
			{
				SearchSettings.Log.Error(string.Format("Could not get named indexes for uri '{0}'. Message: {1}{2}", text, ex.Message, ex.StackTrace));
			}
			return namedIndexes;
		}

        #region Customized
        // Hieu Le: Added sort param
        protected internal virtual SearchResults GetSearchResults(string query, string namedIndexingService, Collection<string> namedIndexes, int offset, int limit, Collection<string> sortFields)
		{
			var dictionary = new Dictionary<string, string>();
			var results = new SearchResults();
			var namedIndexingServiceElement = SearchSettings.GetNamedIndexingServiceElement(namedIndexingService);
			
            string text = string.Empty;
			if (namedIndexes != null && namedIndexes.Count > 0)
			{
				foreach (string current in namedIndexes)
				{
					text = text + current + "|";
				}
				text = text.Substring(0, text.LastIndexOf("|", StringComparison.Ordinal));
			}

            #region Customized
            // Hieu Le: Build sort text
		    var sortFieldsTextBuilder = new StringBuilder();
            if (sortFields != null && sortFields.Count > 0)
            {
                foreach (string sortField in sortFields)
                {
                    sortFieldsTextBuilder.Append(sortField);
                    sortFieldsTextBuilder.Append("|");
                }
                if (sortFieldsTextBuilder.Length > 0)
                {
                    sortFieldsTextBuilder.Remove(sortFieldsTextBuilder.Length - 1, 1);
                }
            }
            #endregion

            dictionary.Add("{q}", query);
			dictionary.Add("{namedindexes}", text);
			dictionary.Add("{accesskey}", namedIndexingServiceElement.AccessKey);
			dictionary.Add("{offset}", SearchSettings.Config.UseIndexingServicePaging ? offset.ToString(CultureInfo.InvariantCulture.NumberFormat) : "0");
			dictionary.Add("{limit}", SearchSettings.Config.UseIndexingServicePaging ? limit.ToString(CultureInfo.InvariantCulture.NumberFormat) : SearchSettings.Config.MaxHitsFromIndexingService.ToString(CultureInfo.InvariantCulture.NumberFormat));

            // Customized: Added sort to dictionary
            dictionary.Add("{sort}", sortFieldsTextBuilder.ToString());

			string text2 = SearchSettings.Config.SearchUriTemplate;
			foreach (string current2 in dictionary.Keys)
			{
				text2 = text2.Replace(current2, HttpUtility.UrlEncode(dictionary[current2]));
			}
			text2 = namedIndexingServiceElement.BaseUri + text2;
			XmlTextReader xmlReader = null;
			SearchSettings.Log.Debug(string.Format("Start get search results from service with url '{0}'", text2));
			OnSearchRequestSending(this, new EventArgs());
			try
			{
				MakeHttpRequest(text2, namedIndexingServiceElement, "GET", null, delegate(WebResponse response)
				{
					xmlReader = new XmlTextReader(response.GetResponseStream());
					xmlReader.Normalization = false;
					SyndicationFeed feed = SyndicationFeed.Load(xmlReader);
					results = PopulateSearchResultsFromFeed(feed, offset, limit);
				});
				OnSearchRequestSent(this, new EventArgs());
			}
			catch (Exception ex)
			{
				SearchSettings.Log.Error(string.Format("Could not get search results for uri '{0}'. Message: {1}{2}", text2, ex.Message, ex.StackTrace));
				return results;
			}
			SearchSettings.Log.Debug(string.Format("End get search results", new object[0]));
			return results;
		}
        #endregion

        private static SearchResults PopulateSearchResultsFromFeed(SyndicationFeed feed, int offset, int limit)
		{
			SearchResults searchResults = new SearchResults();
			int totalHits = 0;
			int.TryParse(GetAttributeValue(feed, SearchSettings.Config.SyndicationFeedAttributeNameTotalHits), out totalHits);
			string attributeValue = GetAttributeValue(feed, SearchSettings.Config.SyndicationFeedAttributeNameVersion);
			foreach (SyndicationItem current in feed.Items)
			{
				try
				{
					IndexResponseItem indexResponseItem = new IndexResponseItem(current.Id);
					indexResponseItem.Title = ((current.Title != null) ? current.Title.Text : null);
					indexResponseItem.DisplayText = (((TextSyndicationContent)current.Content != null) ? ((TextSyndicationContent)current.Content).Text : null);
					indexResponseItem.Created = current.PublishDate.DateTime;
					indexResponseItem.Modified = current.LastUpdatedTime.DateTime;
					indexResponseItem.Uri = current.BaseUri;
					indexResponseItem.Culture = GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameCulture);
					indexResponseItem.ItemType = GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameType);
					indexResponseItem.NamedIndex = GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameNamedIndex);
					indexResponseItem.Metadata = GetElementValue(current, SearchSettings.Config.SyndicationItemElementNameMetadata);
					DateTime value;
					if (DateTime.TryParse(GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNamePublicationEnd), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value))
					{
						indexResponseItem.PublicationEnd = new DateTime?(value);
					}
					DateTime value2;
					if (DateTime.TryParse(GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNamePublicationStart), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value2))
					{
						indexResponseItem.PublicationStart = new DateTime?(value2);
					}
					float num = 1f;
					indexResponseItem.BoostFactor = (float.TryParse(GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameBoostFactor), out num) ? num : 1f);
					Uri uri = null;
					indexResponseItem.DataUri = (Uri.TryCreate(GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameDataUri), UriKind.RelativeOrAbsolute, out uri) ? uri : null);
					float num2 = 0f;
					indexResponseItem.Score = (float.TryParse(GetAttributeValue(current, SearchSettings.Config.SyndicationItemAttributeNameScore), out num2) ? num2 : 0f);
					AddAuthorsToIndexItem(current, indexResponseItem);
					AddCategoriesToIndexItem(current, indexResponseItem);
					AddACLToIndexItem(current, indexResponseItem);
					AddVirtualPathToIndexItem(current, indexResponseItem);
					if (SearchResultFilterHandler.Include(indexResponseItem))
					{
						searchResults.IndexResponseItems.Add(indexResponseItem);
					}
				}
				catch (Exception ex)
				{
					SearchSettings.Log.Error(string.Format("Could not populate search results for syndication item with id '{0}'. Message: {1}{2}", current.Id, ex.Message, ex.StackTrace));
				}
			}
			if (SearchSettings.Config.UseIndexingServicePaging)
			{
				searchResults.TotalHits = totalHits;
				searchResults.Version = attributeValue;
				return searchResults;
			}
			SearchResults searchResults2 = new SearchResults();
			searchResults2.TotalHits = searchResults.IndexResponseItems.Count;
			foreach (IndexResponseItem current2 in searchResults.IndexResponseItems.Skip(offset).Take(limit))
			{
				searchResults2.IndexResponseItems.Add(current2);
			}
			searchResults2.Version = attributeValue;
			return searchResults2;
		}
		private static void AddAuthorsToIndexItem(SyndicationItem syndicationItem, IndexItemBase item)
		{
			if (syndicationItem.Authors != null)
			{
				foreach (SyndicationPerson current in syndicationItem.Authors)
				{
					item.Authors.Add(current.Name);
				}
			}
		}
		private static void AddCategoriesToIndexItem(SyndicationItem syndicationItem, IndexItemBase item)
		{
			if (syndicationItem.Categories != null)
			{
				foreach (SyndicationCategory current in syndicationItem.Categories)
				{
					item.Categories.Add(current.Name);
				}
			}
		}
		private static void AddACLToIndexItem(SyndicationItem syndicationItem, IndexItemBase item)
		{
			Collection<XElement> collection = syndicationItem.ElementExtensions.ReadElementExtensions<XElement>(SearchSettings.Config.SyndicationItemElementNameAcl, SearchSettings.Config.XmlQualifiedNamespace);
			if (collection.Count > 0)
			{
				XElement xElement = collection.ElementAt(0);
				foreach (XElement current in xElement.Elements())
				{
					item.AccessControlList.Add(current.Value);
				}
			}
		}
		private static void AddVirtualPathToIndexItem(SyndicationItem syndicationItem, IndexItemBase item)
		{
			Collection<XElement> collection = syndicationItem.ElementExtensions.ReadElementExtensions<XElement>(SearchSettings.Config.SyndicationItemElementNameVirtualPath, SearchSettings.Config.XmlQualifiedNamespace);
			if (collection.Count > 0)
			{
				XElement xElement = collection.ElementAt(0);
				foreach (XElement current in xElement.Elements())
				{
					item.VirtualPathNodes.Add(current.Value);
				}
			}
		}
		private static string GetAttributeValue(SyndicationItem syndicationItem, string attributeName)
		{
			string result = string.Empty;
			if (syndicationItem.AttributeExtensions.ContainsKey(new XmlQualifiedName(attributeName, SearchSettings.Config.XmlQualifiedNamespace)))
			{
				result = syndicationItem.AttributeExtensions[new XmlQualifiedName(attributeName, SearchSettings.Config.XmlQualifiedNamespace)];
			}
			return result;
		}
		private static string GetAttributeValue(SyndicationFeed syndicationFeed, string attributeName)
		{
			string result = string.Empty;
			if (syndicationFeed.AttributeExtensions.ContainsKey(new XmlQualifiedName(attributeName, SearchSettings.Config.XmlQualifiedNamespace)))
			{
				result = syndicationFeed.AttributeExtensions[new XmlQualifiedName(attributeName, SearchSettings.Config.XmlQualifiedNamespace)];
			}
			return result;
		}
		private static string GetElementValue(SyndicationItem syndicationItem, string elementName)
		{
			Collection<string> collection = syndicationItem.ElementExtensions.ReadElementExtensions<string>(elementName, SearchSettings.Config.XmlQualifiedNamespace);
			string result = "";
			if (collection.Count > 0)
			{
				result = syndicationItem.ElementExtensions.ReadElementExtensions<string>(elementName, SearchSettings.Config.XmlQualifiedNamespace).ElementAt(0);
			}
			return result;
		}
		private static void CopyStream(Stream input, Stream output)
		{
			byte[] array = new byte[32768];
			while (true)
			{
				int num = input.Read(array, 0, array.Length);
				if (num <= 0)
				{
					break;
				}
				output.Write(array, 0, num);
			}
		}
        //private static void MakeHttpRequest(string url, NamedIndexingServiceElement namedIndexingServiceElement, string method, System.IO.Stream postData, System.Action<WebResponse> responseHandler)
        //{
        //    HttpWebRequest httpWebRequest = WebRequest.Create(url) as HttpWebRequest;
        //    httpWebRequest.UseDefaultCredentials = false;
        //    if (httpWebRequest != null)
        //    {
        //        HttpWebRequest httpWebRequest2 = httpWebRequest;
        //        X509Certificate2 clientCertificate = namedIndexingServiceElement.GetClientCertificate();
        //        if (clientCertificate != null)
        //        {
        //            httpWebRequest2.ClientCertificates.Add(clientCertificate);
        //        }
        //        if (namedIndexingServiceElement.CertificateAllowUntrusted)
        //        {
        //            ServicePointManager.ServerCertificateValidationCallback = ((object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true);
        //        }
        //    }
        //    httpWebRequest.Method = method;
        //    if (method == "POST" || method == "DELETE" || method == "PUT")
        //    {
        //        httpWebRequest.ContentType = "application/xml";
        //        if (postData != null)
        //        {
        //            httpWebRequest.ContentLength = postData.Length;
        //            System.IO.Stream requestStream = httpWebRequest.GetRequestStream();
        //            RequestHandler.CopyStream(postData, requestStream);
        //            requestStream.Close();
        //        }
        //        else
        //        {
        //            httpWebRequest.ContentLength = 0L;
        //        }
        //        WebResponse response = httpWebRequest.GetResponse();
        //        if (responseHandler != null)
        //        {
        //            responseHandler(response);
        //        }
        //        response.Close();
        //        return;
        //    }
        //    if (method == "GET")
        //    {
        //        httpWebRequest.ContentType = "application/xml";
        //        WebResponse response2 = httpWebRequest.GetResponse();
        //        if (responseHandler != null)
        //        {
        //            responseHandler(response2);
        //        }
        //        response2.Close();
        //    }
        //}

        private static void MakeHttpRequest(string url, NamedIndexingServiceElement namedIndexingServiceElement, string method, Stream postData, Action<WebResponse> responseHandler)
        {
            string res = String.Empty;

            WebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request is HttpWebRequest)
            {
                HttpWebRequest hwr = (HttpWebRequest)request;
                X509Certificate2 cert = namedIndexingServiceElement.GetClientCertificate();
                if (cert != null)
                {
                    hwr.ClientCertificates.Add(cert);
                }
                if (namedIndexingServiceElement.CertificateAllowUntrusted)
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        return true;
                    };
                }
            }

            request.Method = method;
            if (method == "POST" || method == "DELETE" || method == "PUT")
            {
                request.ContentType = "application/xml";
                if (postData != null)
                {
                    request.ContentLength = postData.Length;

                    Stream dataStream = request.GetRequestStream();
                    CopyStream(postData, dataStream);
                    dataStream.Close();
                }
                else
                {
                    request.ContentLength = 0;
                }

                // Get the response.
                WebResponse response = request.GetResponse();
                if (responseHandler != null)
                    responseHandler(response);
                response.Close();
            }
            else if (method == "GET")
            {
                request.ContentType = "application/xml";

                WebResponse response = request.GetResponse();
                if (responseHandler != null)
                    responseHandler(response);
                response.Close();
            }
        }

		internal static void OnUpdateRequestSent(object sender, EventArgs e)
		{
			if (UpdateRequestSent != null)
			{
				UpdateRequestSent(sender, e);
			}
		}
		internal static void OnUpdateRequestSending(object sender, EventArgs e)
		{
			if (UpdateRequestSending != null)
			{
				UpdateRequestSending(sender, e);
			}
		}
		internal static void OnResetRequestSent(object sender, EventArgs e)
		{
			if (ResetRequestSent != null)
			{
				ResetRequestSent(sender, e);
			}
		}
		internal static void OnResetRequestSending(object sender, EventArgs e)
		{
			if (ResetRequestSending != null)
			{
				ResetRequestSending(sender, e);
			}
		}
		internal static void OnSearchRequestSending(object sender, EventArgs e)
		{
			if (SearchRequestSending != null)
			{
				SearchRequestSending(sender, e);
			}
		}
		internal static void OnSearchRequestSent(object sender, EventArgs e)
		{
			if (SearchRequestSent != null)
			{
				SearchRequestSent(sender, e);
			}
		}
	}
}
