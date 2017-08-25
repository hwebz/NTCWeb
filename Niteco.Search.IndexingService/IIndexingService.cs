using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;
using Niteco.Search.IndexingService.Custom;

namespace Niteco.Search.IndexingService
{
    [ServiceContract(Namespace = IndexingServiceConstants.Namespace), ServiceKnownType(typeof(Atom10FeedFormatter)), ServiceKnownType(typeof(SyndicationFeedFormatter))]
	public interface IIndexingService
	{
		[OperationContract, WebInvoke(Method = "POST", UriTemplate = "reset/?namedindex={namedindex}&accessKey={accessKey}", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
		void ResetIndex(string namedIndex, string accessKey);
		[OperationContract, WebInvoke(Method = "POST", UriTemplate = "update/?accessKey={accessKey}", ResponseFormat = WebMessageFormat.Xml, RequestFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
		void UpdateIndex(string accessKey, SyndicationFeedFormatter formatter);

        #region Customized
        // Hieu Le: Added sort field to query string
        [OperationContract, WebGet(UriTemplate = "search/?q={q}&namedindexes={namedindexes}&format=xml&offset={offset}&limit={limit}&accesskey={accesskey}&sort={sort}", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
		SyndicationFeedFormatter GetSearchResultsXml(string q, string namedIndexes, string offset, string limit, string accessKey, string sort);
        [OperationContract, WebGet(UriTemplate = "search/?q={q}&namedindexes={namedindexes}&format=json&offset={offset}&limit={limit}&accesskey={accesskey}&sort={sort}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
		SyndicationFeedFormatter GetSearchResultsJson(string q, string namedIndexes, string offset, string limit, string accessKey, string sort);
        #endregion
        [OperationContract, WebGet(UriTemplate = "namedindexes/?accesskey={accesskey}", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Bare)]
		SyndicationFeedFormatter GetNamedIndexes(string accessKey);
	}
}
