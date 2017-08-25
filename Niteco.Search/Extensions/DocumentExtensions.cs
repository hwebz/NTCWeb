using System;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using Lucene.Net.Documents;
using Niteco.Common.Search;


namespace Niteco.Search.Extensions
{
    public static class DocumentExtensions
    {
        public static T GetContent<T>(this Document document) where T : IContent
        {
            // EPiServer Search adds a field called 'EPISERVER_SEARCH_ID' which contains the content GUID
            var fieldValue = document.Get(SearchSettings.Config.IndexingServiceFieldNameId);

            if (string.IsNullOrWhiteSpace(fieldValue))
            {
                throw new NotSupportedException(string.Format("Specified document did not have a '{0}' field value", SearchSettings.Config.IndexingServiceFieldNameId));
            }

            var fieldValueFragments = fieldValue.Split('|'); // Field value is either 'GUID|language' or just a GUID

            Guid contentGuid;

            if (!Guid.TryParse(fieldValueFragments[0], out contentGuid))
            {
                throw new NotSupportedException("Expected first part of ID field to be a valid GUID");
            }

            return ServiceLocator.Current.GetInstance<IContentRepository>().Get<T>(contentGuid, LanguageSelector.AutoDetect());
        }

        public static bool IsUnifiedFileDocument(this Document document)
        {
            var underlyingTypes = document.Get(SearchSettings.Config.IndexingServiceFieldNameType);

            return !string.IsNullOrWhiteSpace(underlyingTypes) && underlyingTypes.Contains("UnifiedFile");
        }
    }
}