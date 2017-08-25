using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework;
using EPiServer.Framework.Blobs;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.SpecializedProperties;
using EPiServer.Web;
using log4net;
using Lucene.Net.Search;
using Niteco.Common.Search;
using Niteco.Common.Search.Custom;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Queries.Lucene;

namespace Niteco.Search
{
    [ServiceConfiguration(ServiceType = typeof(IReIndexable), FactoryMember = "CreateInstance"), ServiceConfiguration(ServiceType = typeof(LuceneContentSearchHandler), FactoryMember = "CreateInstance")]
    public class LuceneContentSearchHandler : IReIndexable
    {
        public const string InvariantCultureIndexedName = "iv";
        private const string IgnoreItemSearchId = "<IgnoreItemId>";
        internal const string ItemTypeSeparator = " ";
        internal const char SearchItemIdSeparator = '|';
        private static readonly ILog log = LogManager.GetLogger(typeof(LuceneContentSearchHandler));
        internal static readonly string BaseItemType = LuceneContentSearchHandler.GetItemTypeSection<IContent>();
        private readonly IContentTypeRepository contentTypeRepository;
        private readonly IContentRepository contentRepository;
        private readonly SearchHandler searchHandler;
        private readonly LanguageSelectorFactory languageSelectorFactory;
        private readonly Collection<string> namedIndexes;
        private readonly SearchIndexConfig searchIndexConfig;
        public virtual bool ServiceActive
        {
            get;
            set;
        }

        public string NamedIndex
        {
            get
            {
                if (this.searchIndexConfig == null)
                {
                    return null;
                }
                return this.searchIndexConfig.CMSNamedIndex;
            }
        }

        public string NamedIndexingService
        {
            get
            {
                if (this.searchIndexConfig == null)
                {
                    return null;
                }
                return this.searchIndexConfig.NamedIndexingService;
            }
        }

        public LuceneContentSearchHandler(SearchHandler searchHandler, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, LanguageSelectorFactory languageSelectorFactory, SearchIndexConfig searchIndexConfig)
        {
            Validator.ThrowIfNull("searchHandler", searchHandler);
            Validator.ThrowIfNull("contentRepository", contentRepository);
            Validator.ThrowIfNull("contentTypeRepository", contentTypeRepository);
            Validator.ThrowIfNull("languageSelectorFactory", languageSelectorFactory);
            this.searchHandler = searchHandler;
            this.contentRepository = contentRepository;
            this.contentTypeRepository = contentTypeRepository;
            this.languageSelectorFactory = languageSelectorFactory;
            this.searchIndexConfig = searchIndexConfig;
            if (this.NamedIndex != null)
            {
                this.namedIndexes = new System.Collections.ObjectModel.Collection<string>();
                this.namedIndexes.Add(this.NamedIndex);
            }
        }

        public static LuceneContentSearchHandler CreateInstance(SearchHandler searchHandler, IContentRepository contentRepository, IContentTypeRepository contentTypeRepository, LanguageSelectorFactory languageSelectorFactory, SearchIndexConfig searchIndexConfig)
        {
            return new LuceneContentSearchHandler(searchHandler, contentRepository, contentTypeRepository, languageSelectorFactory, searchIndexConfig)
            {
                ServiceActive = SearchSettings.Config.Active
            };
        }

        public virtual void IndexPublishedContent()
        {
            if (!this.ServiceActive)
            {
                return;
            }
            var slimContentReader = new SlimContentReader(this.contentRepository, this.languageSelectorFactory, ContentReference.RootPage, delegate(IContent c)
            {
                var searchable = c as ISearchable;
                return searchable == null || searchable.AllowIndexChildren;
            });
            while (slimContentReader.Next())
            {
                if (!slimContentReader.Current.ContentLink.CompareToIgnoreWorkID(ContentReference.RootPage))
                {
                    var versionable = slimContentReader.Current as IVersionable;
                    if (versionable == null || versionable.Status == VersionStatus.Published)
                    {
                        this.UpdateItem(slimContentReader.Current);
                    }
                }
            }
        }

        #region Customized
        // Hieu Le - 2013
        public virtual void UpdateItem(IContent contentItem)
        {
            if (contentItem == null)
            {
                return;
            }

            if (!this.ServiceActive)
            {
                return;
            }

            var searchableItem = contentItem as ISearchable;
            if (searchableItem == null)
            {
                return;
            }

            string searchId = LuceneContentSearchHandler.GetSearchId(contentItem);

            var item = new IndexRequestItem(searchId, searchableItem.IsSearchable && this.IsInSearchableBranch(contentItem) ? IndexAction.Update : IndexAction.Remove);
            if (item.IndexAction != IndexAction.Remove)
            {
                item.AutoUpdateVirtualPath = true;
                this.ConvertContentToIndexItem(contentItem, item);
            }

            this.searchHandler.UpdateIndex(item, this.NamedIndexingService);

            this.UpdateDescendantItemsOf(contentItem);
        }

        private void RemoveDescendantItemsOf(IContent contentItem)
        {
            foreach (var childContent in this.contentRepository.GetChildren<IContent>(contentItem.ContentLink))
            {
                var item = new IndexRequestItem(LuceneContentSearchHandler.GetSearchId(childContent), IndexAction.Remove);
                this.searchHandler.UpdateIndex(item, this.NamedIndexingService);
                this.RemoveDescendantItemsOf(childContent);
            }
        }

        private void UpdateDescendantItemsOf(IContent contentItem)
        {
            foreach (var childContent in this.contentRepository.GetChildren<IContent>(contentItem.ContentLink))
            {
                var searchableItem = childContent as ISearchable;
                if (searchableItem == null)
                {
                    this.UpdateDescendantItemsOf(childContent);
                }
                else
                {
                    string searchId = LuceneContentSearchHandler.GetSearchId(childContent);
                    var item = new IndexRequestItem(searchId, searchableItem.IsSearchable && this.IsInSearchableBranch(childContent) ? IndexAction.Update : IndexAction.Remove);
                    if (item.IndexAction != IndexAction.Remove)
                    {
                        item.AutoUpdateVirtualPath = true;
                        this.ConvertContentToIndexItem(childContent, item);
                    }
                    this.searchHandler.UpdateIndex(item, this.NamedIndexingService);

                    if (searchableItem.AllowIndexChildren)
                    {
                        this.UpdateDescendantItemsOf(childContent);
                    }
                    else
                    {
                        this.RemoveDescendantItemsOf(childContent);
                    }
                }
            }
        }

        private bool IsInSearchableBranch(IContent contentItem)
        {
            // If the item is searchable then we need check its parent
            while (!contentItem.ContentLink.CompareToIgnoreWorkID(ContentReference.RootPage))
            {
                if (contentItem.ContentLink.CompareToIgnoreWorkID(ContentReference.WasteBasket))
                {
                    return false;
                }

                contentItem = this.contentRepository.Get<IContent>(contentItem.ParentLink);

                var searchableItem = contentItem as ISearchable;
                if (searchableItem != null && !searchableItem.AllowIndexChildren)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual SearchResults GetSearchResults<T>(GroupQuery groupQuery, ContentReference root, int page, int pageSize, Collection<SortField> sortFields, bool filterOnAccess) where T : IContent
        {
            if (!this.ServiceActive)
            {
                return null;
            }
            //var groupQuery = new GroupQuery(LuceneOperator.AND);
            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            groupQuery.QueryExpressions.Add(new FieldQuery(ContentLanguage.PreferredCulture.Name, Field.Culture));
            if (!ContentReference.IsNullOrEmpty(root))
            {
                var virtualPathQuery = new VirtualPathQuery();
                virtualPathQuery.AddContentNodes(root, this.contentRepository);
                groupQuery.QueryExpressions.Add(virtualPathQuery);
            }

            if (filterOnAccess)
            {
                var accessControlListQuery = new AccessControlListQuery();
                accessControlListQuery.AddAclForUser(PrincipalInfo.Current, HttpContext.Current);
                groupQuery.QueryExpressions.Add(accessControlListQuery);
            }

            Collection<string> sortFieldCollection = null;
            if (sortFields != null && sortFields.Count > 0)
            {
                sortFieldCollection = new Collection<string>();
                foreach (var sortField in sortFields)
                {
                    sortFieldCollection.Add(string.Format("{0},{1},{2}", sortField.Field, sortField.Type, sortField.Reverse));
                }
            }

            return this.searchHandler.GetSearchResults(groupQuery, this.NamedIndexingService, this.namedIndexes, page, pageSize, sortFieldCollection);
        }

        #endregion

        public virtual void MoveItem(ContentReference contentLink)
        {
            if (!this.ServiceActive)
            {
                return;
            }
            if (ContentReference.IsNullOrEmpty(contentLink))
            {
                return;
            }
            IContent content;
            if (!this.contentRepository.TryGet(contentLink, this.languageSelectorFactory.MasterLanguage(), out content))
            {
                return;
            }
            string searchId = LuceneContentSearchHandler.GetSearchId(content);
            var indexRequestItem = new IndexRequestItem(searchId, IndexAction.Update);
            indexRequestItem.AutoUpdateVirtualPath = true;
            this.ConvertContentToIndexItem(content, indexRequestItem);
            this.searchHandler.UpdateIndex(indexRequestItem, this.NamedIndexingService);
        }

        public virtual void RemoveItemsByVirtualPath(System.Collections.Generic.ICollection<string> virtualPathNodes)
        {
            Validator.ThrowIfNull("virtualPathNodes", virtualPathNodes);
            if (!ServiceActive || virtualPathNodes.Count == 0)
            {
                return;
            }
            var indexRequestItem = new IndexRequestItem(IgnoreItemSearchId, IndexAction.Remove);
            foreach (string current in virtualPathNodes)
            {
                indexRequestItem.VirtualPathNodes.Add(current);
            }
            indexRequestItem.AutoUpdateVirtualPath = true;
            indexRequestItem.NamedIndex = this.NamedIndex;
            this.searchHandler.UpdateIndex(indexRequestItem, this.NamedIndexingService);
        }

        public virtual void RemoveLanguageBranch(IContent contentItem)
        {
            Validator.ThrowIfNull("contentItem", contentItem);
            if (!this.ServiceActive)
            {
                return;
            }
            string searchId = LuceneContentSearchHandler.GetSearchId(contentItem);
            var indexRequestItem = new IndexRequestItem(searchId, IndexAction.Remove);
            indexRequestItem.NamedIndex = this.NamedIndex;
            this.searchHandler.UpdateIndex(indexRequestItem, this.NamedIndexingService);
        }

        public static System.Collections.Generic.ICollection<string> GetVirtualPathNodes(ContentReference contentLink, IContentLoader contentLoader)
        {
            Validator.ThrowIfNull("contentLink", contentLink);
            Validator.ThrowIfNull("contentLoader", contentLoader);
            var collection = new Collection<string>();
            foreach (IContent current in contentLoader.GetAncestors(contentLink).Reverse())
            {
                collection.Add(current.ContentGuid.ToString());
            }
            var content = contentLoader.Get<IContent>(contentLink);
            collection.Add(content.ContentGuid.ToString());
            return collection;
        }

        public string GetItemType<T>()
        {
            return this.GetItemType(typeof(T));
        }

        public virtual string GetItemType(Type contentType)
        {
            Validator.ThrowIfNull("contentType", contentType);
            var stringBuilder = new System.Text.StringBuilder(LuceneContentSearchHandler.GetItemTypeSection(contentType));
            if (contentType.IsClass)
            {
                while (contentType.BaseType != typeof(object))
                {
                    contentType = contentType.BaseType;
                    stringBuilder.Append(" ");
                    stringBuilder.Append(LuceneContentSearchHandler.GetItemTypeSection(contentType));
                }
            }
            stringBuilder.Append(" ");
            stringBuilder.Append(LuceneContentSearchHandler.BaseItemType);
            return stringBuilder.ToString();
        }

        public static string GetItemTypeSection<T>()
        {
            return LuceneContentSearchHandler.GetItemTypeSection(typeof(T));
        }

        public static string GetItemTypeSection(System.Type type)
        {
            Validator.ThrowIfNull("type", type);
            return type.FullName + "," + type.Assembly.GetName().Name;
        }

        public T GetContent<T>(IndexItemBase indexItem) where T : IContent
        {
            return this.GetContent<T>(indexItem, false);
        }

        public virtual T GetContent<T>(IndexItemBase indexItem, bool filterOnCulture) where T : IContent
        {
            if (indexItem == null || string.IsNullOrEmpty(indexItem.Id))
            {
                return default(T);
            }
            var input = indexItem.Id.Split(new[]
			{
				'|'
			}).FirstOrDefault();
            System.Guid guid;
            if (System.Guid.TryParse(input, out guid))
            {
                ILanguageSelector selector = filterOnCulture ? this.languageSelectorFactory.AutoDetect(false) : this.GetLanguageSelector(indexItem.Culture);
                try
                {
                    //return this.contentRepository.Get<T>(guid, selector);
                    return contentRepository.Get<T>(guid);
                }
                catch (ContentNotFoundException exception)
                {
                    LuceneContentSearchHandler.log.Warn(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Search index returned an item with GUID {0:B}, that no longer exists in the content repository.", new object[]
					{
						guid
					}), exception);
                }
            }
            return default(T);
        }

        public virtual SearchResults GetSearchResults<T>(string searchQuery, ContentReference root, int page, int pageSize, Collection<SortField> sortFields, bool filterOnAccess) where T : IContent
        {
            if (!this.ServiceActive)
            {
                return null;
            }
            var groupQuery = new GroupQuery(LuceneOperator.AND);
            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            groupQuery.QueryExpressions.Add(new FieldQuery(ContentLanguage.PreferredCulture.Name, Field.Culture));
            groupQuery.QueryExpressions.Add(new FieldQuery(searchQuery));
            if (!ContentReference.IsNullOrEmpty(root))
            {
                var virtualPathQuery = new VirtualPathQuery();
                virtualPathQuery.AddContentNodes(root, this.contentRepository);
                groupQuery.QueryExpressions.Add(virtualPathQuery);
            }
            if (filterOnAccess)
            {
                var accessControlListQuery = new AccessControlListQuery();
                accessControlListQuery.AddAclForUser(PrincipalInfo.Current, HttpContext.Current);
                groupQuery.QueryExpressions.Add(accessControlListQuery);
            }

            #region Customized
            Collection<string> sortFieldCollection = null;
            if (sortFields != null && sortFields.Count > 0)
            {
                sortFieldCollection = new Collection<string>();
                foreach (var sortField in sortFields)
                {
                    sortFieldCollection.Add(string.Format("{0},{1},{2}", sortField.Field, sortField.Type, sortField.Reverse));
                }
            }
            #endregion
            return this.searchHandler.GetSearchResults(groupQuery, this.NamedIndexingService, this.namedIndexes, page, pageSize, sortFieldCollection);
        }

        public virtual SearchResults GetSearchResults<T>(string searchQuery, int page, int pageSize) where T : IContent
        {
            return this.GetSearchResults<T>(searchQuery, ContentReference.EmptyReference, page, pageSize, null, true);
        }

        public static string GetCultureIdentifier(System.Globalization.CultureInfo culture)
        {
            if (culture == null)
            {
                return string.Empty;
            }
            if (!System.Globalization.CultureInfo.InvariantCulture.Equals(culture))
            {
                return culture.Name;
            }
            return "iv";
        }

        private static string GetSearchId(IContent content)
        {
            System.Globalization.CultureInfo language = null;
            var locale = content as ILocale;
            if (locale != null)
            {
                language = locale.Language;
            }
            return LuceneContentSearchHandler.CreateSearchId(content.ContentGuid, language);
        }

        private static string CreateSearchId(System.Guid contentGuid, System.Globalization.CultureInfo language)
        {
            return string.Format("{0}|{1}", contentGuid, LuceneContentSearchHandler.GetCultureIdentifier(language));
        }

        private ILanguageSelector GetLanguageSelector(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return this.languageSelectorFactory.AutoDetect(true);
            }
            if (languageCode == "iv")
            {
                languageCode = string.Empty;
            }
            return new LanguageSelector(languageCode);
        }

        private void ConvertContentToIndexItem(IContent content, IndexRequestItem item)
        {
            var contentSecurable = content as IContentSecurable;
            var categorizable = content as ICategorizable;
            LuceneContentSearchHandler.AddUriToIndexItem(content, item);
            this.AddMetaDataToIndexItem(content, item);
            this.AddSearchablePropertiesToIndexItem(content, item);
            LuceneContentSearchHandler.AddBinaryStorableToIndexItem(content, item);
            if (contentSecurable != null)
            {
                IContentSecurityDescriptor contentSecurityDescriptor = contentSecurable.GetContentSecurityDescriptor();
                if (contentSecurityDescriptor != null && contentSecurityDescriptor.Entries != null)
                {
                    LuceneContentSearchHandler.AddReadAccessToIndexItem(contentSecurityDescriptor.Entries, item);
                }
            }
            else
            {
                item.AccessControlList.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture, "G:{0}", new object[]
				{
					EveryoneRole.RoleName
				}));
            }
            if (categorizable != null)
            {
                LuceneContentSearchHandler.AddCategoriesToIndexItem(categorizable.Category, item);
            }
            this.AddVirtualPathToIndexItem(content.ContentLink, item);
            LuceneContentSearchHandler.AddItemStatusToIndexItem(item);
            LuceneContentSearchHandler.AddExpirationToIndexItem(content, item);
            item.NamedIndex = this.NamedIndex;
        }

        private static void AddBinaryStorableToIndexItem(IContent content, IndexRequestItem item)
        {
            var binaryStorable = content as IBinaryStorable;
            if (binaryStorable != null && binaryStorable.BinaryData is FileBlob)
            {
                item.DataUri = new Uri(((FileBlob)binaryStorable.BinaryData).FilePath);
            }
        }

        private static void AddUriToIndexItem(IContent content, IndexRequestItem item)
        {
            string url = PermanentLinkUtility.GetPermanentLinkVirtualPath(content.ContentGuid, ".aspx");
            var locale = content as ILocale;
            if (locale != null && locale.Language != null)
            {
                url = UriSupport.AddLanguageSelection(url, locale.Language.Name);
            }
            item.Uri = new Url(url).Uri;
        }

        private void AddMetaDataToIndexItem(IContent content, IndexRequestItem item)
        {
            var locale = content as ILocale;
            var changeTrackable = content as IChangeTrackable;
            item.Title = content.Name;
            item.Created = ((changeTrackable != null) ? changeTrackable.Created : System.DateTime.MinValue);
            item.Modified = ((changeTrackable != null) ? changeTrackable.Changed : System.DateTime.MinValue);
            item.Culture = ((locale != null) ? LuceneContentSearchHandler.GetCultureIdentifier(locale.Language) : string.Empty);
            item.ItemType = this.GetItemType(content.GetOriginalType());
            item.Authors.Add((changeTrackable != null) ? changeTrackable.CreatedBy : string.Empty);
        }

        private void AddSearchablePropertiesToIndexItem(IContent content, IndexRequestItem item)
        {
            item.DisplayText = string.Join(System.Environment.NewLine, this.GetSearchablePropertyValues(content, content.ContentTypeID).ToArray());
        }

        private System.Collections.Generic.IEnumerable<string> GetSearchablePropertyValues(IContentData contentData, int contentTypeID)
        {
            return this.GetSearchablePropertyValues(contentData, this.contentTypeRepository.Load(contentTypeID));
        }

        private System.Collections.Generic.IEnumerable<string> GetSearchablePropertyValues(IContentData contentData, System.Type modelType)
        {
            return this.GetSearchablePropertyValues(contentData, this.contentTypeRepository.Load(modelType));
        }

        private System.Collections.Generic.IEnumerable<string> GetSearchablePropertyValues(IContentData contentData, ContentType contentType)
        {
            if (contentType != null)
            {
                foreach (PropertyDefinition current in
                    from d in contentType.PropertyDefinitions
                    where d.Searchable || typeof(IPropertyBlock).IsAssignableFrom(d.Type.DefinitionType)
                    select d)
                {
                    PropertyData propertyData = contentData.Property[current.Name];
                    var propertyBlock = propertyData as IPropertyBlock;
                    if (propertyBlock != null)
                    {
                        foreach (string current2 in this.GetSearchablePropertyValues(propertyBlock.Block, propertyBlock.BlockType))
                        {
                            yield return current2;
                        }
                    }
                    else
                    {
                        yield return propertyData.ToWebString();
                    }
                }
            }
        }

        private static void AddCategoriesToIndexItem(CategoryList categories, IndexRequestItem item)
        {
            if (categories == null || categories.Count == 0)
            {
                return;
            }
            foreach (int current in categories)
            {
                item.Categories.Add(current.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void AddVirtualPathToIndexItem(ContentReference contentLink, IndexRequestItem item)
        {
            if (!ContentReference.IsNullOrEmpty(contentLink))
            {
                foreach (string current in LuceneContentSearchHandler.GetVirtualPathNodes(contentLink, this.contentRepository))
                {
                    item.VirtualPathNodes.Add(current);
                }
            }
        }

        private static void AddItemStatusToIndexItem(IndexRequestItem item)
        {
            item.ItemStatus = ItemStatus.Approved;
        }

        private static void AddExpirationToIndexItem(IContent content, IndexRequestItem item)
        {
            var versionable = content as IVersionable;
            if (versionable != null && versionable.StopPublish != System.DateTime.MaxValue)
            {
                item.PublicationEnd = versionable.StopPublish;
            }
        }

        internal static void AddReadAccessToIndexItem(System.Collections.Generic.IEnumerable<AccessControlEntry> acl, IndexRequestItem item)
        {
            foreach (AccessControlEntry current in acl)
            {
                if ((current.Access & AccessLevel.Read) == AccessLevel.Read)
                {
                    item.AccessControlList.Add(string.Format("{0}:{1}", (current.EntityType == SecurityEntityType.User) ? "U" : "G", current.Name));
                }
            }
        }

        public void ReIndex()
        {
            this.IndexPublishedContent();
        }
    }
}
