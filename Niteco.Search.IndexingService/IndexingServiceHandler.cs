using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Niteco.Search.IndexingService.Custom;
using Niteco.Search.IndexingService.FieldSerializers;

namespace Niteco.Search.IndexingService
{
	public class IndexingServiceHandler
	{
		private class DataUriQueueItem
		{
			private SyndicationItem _item;
			private NamedIndex _namedIndex;
			internal DataUriQueueItem(SyndicationItem item, NamedIndex namedIndex)
			{
				this._item = item;
				this._namedIndex = namedIndex;
			}
			internal void Do()
			{
				IndexingServiceHandler.Instance.HandleDataUri(this._item, this._namedIndex);
			}
		}
		private const string IgnoreItemId = "<IgnoreItemId>";
		private const IFILTER_INIT FILTERSETTINGS = IFILTER_INIT.IFILTER_INIT_CANON_SPACES | IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES | IFILTER_INIT.IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES | IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY;
		private const int BufferSize = 65536;
		private static IndexingServiceHandler _instance;
		private static TaskQueue _taskQueue;
		public static IndexingServiceHandler Instance
		{
			get
			{
				return IndexingServiceHandler._instance;
			}
			set
			{
				IndexingServiceHandler._instance = value;
			}
		}
		protected IndexingServiceHandler()
		{
		}
		static IndexingServiceHandler()
		{
			IndexingServiceHandler._instance = new IndexingServiceHandler();
			IndexingServiceHandler._taskQueue = new TaskQueue("indexing service data uri callback", 1000.0, System.TimeSpan.FromSeconds(0.0));
		}
		protected internal virtual void UpdateIndex(SyndicationFeed feed)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start processing feed '{0}'", feed.Id));
			foreach (SyndicationItem current in feed.Items)
			{
				string attributeValue = IndexingServiceHandler.GetAttributeValue(current, "NamedIndex");
				if (IndexingServiceHandler.IsValidIndex(attributeValue) && IndexingServiceHandler.IsModifyIndex(attributeValue))
				{
					string text = IndexingServiceHandler.GetAttributeValue(current, "ReferenceId");
					string attributeValue2 = IndexingServiceHandler.GetAttributeValue(current, "IndexAction");
					string attributeValue3 = IndexingServiceHandler.GetAttributeValue(current, "DataUri");
					NamedIndex namedIndex = new NamedIndex(attributeValue, !string.IsNullOrEmpty(text));
					if (string.IsNullOrEmpty(text) && (attributeValue2 == "update" || attributeValue2 == "remove"))
					{
						text = this.GetReferenceIdForItem(current.Id, namedIndex);
						if (!string.IsNullOrEmpty(text))
						{
							IndexingServiceHandler.SetAttributeValue(current, "ReferenceId", text);
							namedIndex = new NamedIndex(attributeValue, true);
						}
					}
					IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start processing feed item '{0}' for '{1}'", current.Id, attributeValue2));
					if (!string.IsNullOrEmpty(attributeValue3))
					{
						Action task = new Action(new IndexingServiceHandler.DataUriQueueItem(current, namedIndex).Do);
						IndexingServiceHandler._taskQueue.Enqueue(task);
						IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Callback for data uri '{0}' enqueued", attributeValue3));
					}
					else
					{
						string a;
						if ((a = attributeValue2) != null)
						{
							if (!(a == "add"))
							{
								if (!(a == "update"))
								{
									if (a == "remove")
									{
										this.Remove(current, namedIndex);
									}
								}
								else
								{
									this.Update(current, namedIndex);
								}
							}
							else
							{
								this.Add(current, namedIndex);
							}
						}
						if (!string.IsNullOrEmpty(text))
						{
							this.UpdateReference(text, current.Id, new NamedIndex(attributeValue));
							IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Updated reference with reference id '{0}' ", text));
						}
					}
				}
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End processing feed item '{0}'", current.Id));
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End processing feed '{0}'", feed.Id));
		}

        #region Customized
        // Hieu Le: Customized sortfields
        protected internal virtual SyndicationFeedFormatter GetSearchResults(string q, string[] namedIndexNames, int offset, int limit, string[] sortFields)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start search with expression: '{0}'", q));
			int num = 0;
			var syndicationFeed = new SyndicationFeed();
			var collection = new Collection<SyndicationItem>();
			var collection2 = new Collection<NamedIndex>();
			if (namedIndexNames != null && namedIndexNames.Length > 0)
            //if (namedIndexNames != null)
			{
				for (int i = 0; i < namedIndexNames.Length; i++)
				{
					string name = namedIndexNames[i];
					var namedIndex = new NamedIndex(name);
					if (!namedIndex.IsValid)
					{
						IndexingServiceSettings.HandleServiceError(string.Format("Named index \"{0}\" is not valid, it does not exist in configuration or has faulty configuration", namedIndex.Name));
						return null;
					}
					collection2.Add(namedIndex);
				}
			}
            else
            {
                NamedIndex namedIndex2 = new NamedIndex();
                if (!namedIndex2.IsValid)
                {
                    IndexingServiceSettings.HandleServiceError(string.Format("Named index \"{0}\" is not valid, it does not exist in configuration or has faulty configuration", namedIndex2.Name));
                    return null;
                }
                collection2.Add(namedIndex2);
            }

            Collection<SortField> sortFieldCollection = null;
		    if (sortFields != null)
		    {
                sortFieldCollection = new Collection<SortField>();
                for (int i = 0; i < sortFields.Length; i++)
                {
                    var parts = sortFields[i].Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        int sortFieldType;
                        bool reverse;
                        if (int.TryParse(parts[1], out sortFieldType) && bool.TryParse(parts[2], out reverse))
                        {
                            sortFieldCollection.Add(new SortField(parts[0], sortFieldType, reverse));
                        }
                    }
                }
		    }
			var scoreDocuments = this.GetScoreDocuments(q, true, collection2, offset, limit, IndexingServiceSettings.MaxHitsForSearchResults, sortFieldCollection, out num);
			int num2 = 0;
			foreach (ScoreDocument current in scoreDocuments)
			{
				SyndicationItem syndicationItemFromDocument = this.GetSyndicationItemFromDocument(current);
				collection.Add(syndicationItemFromDocument);
				num2++;
			}
            syndicationFeed.AttributeExtensions.Add(new XmlQualifiedName("TotalHits", IndexingServiceConstants.Namespace), num.ToString(System.Globalization.CultureInfo.InvariantCulture));
			System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			string value = string.Format(System.Globalization.CultureInfo.InvariantCulture, "EPiServer.Search v.{0}.{1}.{2}.{3}", new object[]
			{
				version.Major.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Minor.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Build.ToString(System.Globalization.CultureInfo.InvariantCulture),
				version.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture)
			});
            syndicationFeed.AttributeExtensions.Add(new XmlQualifiedName("Version", IndexingServiceConstants.Namespace), value);
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End search with expression '{0}'. Returned {1} hits of total {2} with offset {3} and limit {4}", new object[]
			{
				q,
				num2.ToString(System.Globalization.CultureInfo.InvariantCulture),
				num.ToString(System.Globalization.CultureInfo.InvariantCulture),
				offset.ToString(System.Globalization.CultureInfo.InvariantCulture),
				limit.ToString(System.Globalization.CultureInfo.InvariantCulture)
			}));
			syndicationFeed.Items = collection;
			return new Atom10FeedFormatter(syndicationFeed);
		}
        #endregion

        protected internal virtual SyndicationFeedFormatter GetNamedIndexes()
		{
			SyndicationFeed syndicationFeed = new SyndicationFeed();
			Collection<SyndicationItem> collection = new Collection<SyndicationItem>();
			foreach (string current in IndexingServiceSettings.NamedIndexElements.Keys)
			{
				collection.Add(new SyndicationItem
				{
					Title = new TextSyndicationContent(current)
				});
			}
			syndicationFeed.Items = collection;
			return new Atom10FeedFormatter(syndicationFeed);
		}
		protected internal virtual void ResetNamedIndex(string namedIndexName)
		{
			NamedIndex namedIndex = new NamedIndex(namedIndexName);
			if (IndexingServiceSettings.NamedIndexElements.ContainsKey(namedIndexName))
			{
				IndexingServiceHandler.CreateIndex(namedIndex.Name, namedIndex.DirectoryInfo);
				IndexingServiceHandler.CreateIndex(namedIndex.ReferenceName, namedIndex.ReferenceDirectoryInfo);
				return;
			}
			IndexingServiceSettings.HandleServiceError(string.Format("Reset of index: '{0}' failed. Index not found!", namedIndexName));
		}
		protected internal virtual string GetNonFileUriContent(Uri uri)
		{
			return "";
		}
		protected internal virtual string GetFileUriContent(Uri uri)
		{
			return IndexingServiceHandler.GetFileText(uri.LocalPath);
		}
		internal void HandleDataUri(SyndicationItem item, NamedIndex namedIndex)
		{
			string attributeValue = IndexingServiceHandler.GetAttributeValue(item, "DataUri");
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start processing data uri callback for uri '{0}'", attributeValue));
			Uri uri = null;
			if (!Uri.TryCreate(attributeValue, UriKind.RelativeOrAbsolute, out uri))
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Data Uri callback failed. Uri '{0}' is not well formed", attributeValue));
				return;
			}
			string text = string.Empty;
			if (uri.IsFile)
			{
                //Hieu Le: Avoid index breaking if physical file does not exist
			    if (System.IO.File.Exists(uri.LocalPath))
			    {
			        text = this.GetFileUriContent(uri);
			    }
                else
                {
                    IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("File for uri '{0}' does not exist", uri));
                    //IndexingServiceSettings.HandleServiceError(string.Format("File for uri '{0}' does not exist", uri));
                    //return;
                }
			}
			else
			{
				text = this.GetNonFileUriContent(uri);
			}
			if (string.IsNullOrEmpty(text))
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Content for uri '{0}' is empty", uri));
				text = string.Empty;
			}
			string empty = string.Empty;
			string empty2 = string.Empty;
			TextSyndicationContent textSyndicationContent = item.Content as TextSyndicationContent;
			if (textSyndicationContent != null && !string.IsNullOrEmpty(textSyndicationContent.Text))
			{
				IndexingServiceHandler.SplitDisplayTextToMetadata(textSyndicationContent.Text + " " + text, IndexingServiceHandler.GetElementValue(item, "Metadata"), out empty, out empty2);
			}
			else
			{
				IndexingServiceHandler.SplitDisplayTextToMetadata(text, IndexingServiceHandler.GetElementValue(item, "Metadata"), out empty, out empty2);
			}
			item.Content = new TextSyndicationContent(empty);
			IndexingServiceHandler.SetElementValue(item, "Metadata", empty2);
			string attributeValue2 = IndexingServiceHandler.GetAttributeValue(item, "IndexAction");
			if (attributeValue2 == "add")
			{
				this.Add(item, namedIndex);
			}
			else
			{
				if (attributeValue2 == "update")
				{
					this.Update(item, namedIndex);
				}
			}
			string attributeValue3 = IndexingServiceHandler.GetAttributeValue(item, "ReferenceId");
			if (!string.IsNullOrEmpty(attributeValue3))
			{
				this.UpdateReference(attributeValue3, item.Id, new NamedIndex(namedIndex.Name));
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Updated reference with reference id '{0}' ", attributeValue3));
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End data uri callback", new object[0]));
		}
		internal static Lucene.Net.Store.Directory CreateIndex(string name, System.IO.DirectoryInfo directoryInfo)
		{
			Lucene.Net.Store.Directory directory = null;
			IndexingServiceSettings.ReaderWriterLocks[name].EnterWriteLock();
			try
			{
				directory = FSDirectory.Open(directoryInfo);
				using (new IndexWriter(directory, new StandardAnalyzer(IndexingServiceSettings.LuceneVersion), true, IndexWriter.MaxFieldLength.UNLIMITED))
				{
				}
			}
			catch (System.Exception ex)
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Failed to create index for path: '{0}'. Message: {1}{2}'", directoryInfo.FullName, ex.Message, ex.StackTrace));
			}
			finally
			{
				IndexingServiceSettings.ReaderWriterLocks[name].ExitWriteLock();
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Created index for path: '{0}'", directoryInfo.FullName));
			return directory;
		}
		private string GetReferenceIdForItem(string itemId, NamedIndex namedIndex)
		{
			NamedIndex namedIndex2 = new NamedIndex(namedIndex.Name, true);
			Document documentById = this.GetDocumentById(itemId, namedIndex2);
			if (documentById == null)
			{
				return null;
			}
			return documentById.Get(IndexingServiceSettings.ReferenceIdFieldName);
		}
		private Collection<ScoreDocument> GetScoreDocuments(string q, bool excludeNotPublished, Collection<NamedIndex> namedIndexes, int offset, int limit, int maxHits, Collection<SortField> sortFields, out int totalHits)
		{
			Collection<ScoreDocument> collection;
			q = IndexingServiceHandler.PrepareExpression(q, excludeNotPublished);
			if (namedIndexes == null)
			{
				throw new System.ArgumentNullException("namedIndexes");
			}
			if (namedIndexes.Count == 0)
			{
				throw new System.ArgumentException("Called GetScoreDocuments without any named indexes", "namedIndexes");
			}
			if (namedIndexes.Count == 1)
			{
				collection = this.SingleIndexSearch(q, namedIndexes[0], maxHits, sortFields, out totalHits);
			}
			else
			{
				collection = IndexingServiceHandler.MultiIndexSearch(q, namedIndexes, maxHits, sortFields, out totalHits);
			}
			var collection2 = new Collection<ScoreDocument>();
			int num = limit + offset;
			num = ((totalHits < num) ? totalHits : num);
			for (int i = offset; i < num; i++)
			{
				collection2.Add(collection[i]);
			}
			return collection2;
		}
		private void Add(SyndicationItem syndicationItem, NamedIndex namedIndex)
		{
			if (syndicationItem == null)
			{
				return;
			}
			string id = syndicationItem.Id;
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start adding Lucene document with id field: '{0}' to index: '{1}'", id, namedIndex.Name));
			if (string.IsNullOrEmpty(id))
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Failed to add Document. id field is missing.", new object[0]));
				return;
			}
			if (this.DocumentExists(id, namedIndex))
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Document already exists. Skipping.", new object[0]));
				return;
			}
			Document documentFromSyndicationItem = this.GetDocumentFromSyndicationItem(syndicationItem, namedIndex);
			Niteco.Search.IndexingService.IndexingService.OnDocumentAdding(this, new AddUpdateEventArgs(documentFromSyndicationItem, namedIndex.Name));
			this.WriteToIndex(id, documentFromSyndicationItem, namedIndex);
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End adding document with id field: '{0}' to index: '{1}'", id, namedIndex.Name));
			Niteco.Search.IndexingService.IndexingService.OnDocumentAdded(this, new AddUpdateEventArgs(documentFromSyndicationItem, namedIndex.Name));
		}
		private void Update(SyndicationItem item, NamedIndex namedIndex)
		{
			string oldVirtualPath = string.Empty;
			string newVirtualPath = string.Empty;
			if (this.GetAutoUpdateVirtualPathValue(item))
			{
				Document documentById = this.GetDocumentById(item.Id, namedIndex);
				if (documentById != null)
				{
					oldVirtualPath = documentById.Get(IndexingServiceSettings.VirtualPathFieldName);
					newVirtualPath = new VirtualPathFieldStoreSerializer(item).ToFieldStoreValue();
				}
			}
			this.Remove(item.Id, namedIndex, false);
			this.Add(item, namedIndex);
			this.UpdateVirtualPaths(oldVirtualPath, newVirtualPath);
		}
		private void Remove(SyndicationItem item, NamedIndex namedIndex)
		{
			if (!string.Equals(item.Id, "<IgnoreItemId>"))
			{
				this.Remove(item.Id, namedIndex, true);
			}
			if (this.GetAutoUpdateVirtualPathValue(item))
			{
				string virtualPath = new VirtualPathFieldStoreSerializer(item).ToFieldStoreValue();
				this.RemoveByVirtualPath(virtualPath);
			}
		}
		private bool GetAutoUpdateVirtualPathValue(SyndicationItem item)
		{
			bool flag;
			return bool.TryParse(IndexingServiceHandler.GetAttributeValue(item, "AutoUpdateVirtualPath"), out flag) && flag;
		}
		private void Remove(string itemId, NamedIndex namedIndex, bool removeRef)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start deleting Lucene document with id field: '{0}' from index '{1}'", itemId, namedIndex.Name));
			Niteco.Search.IndexingService.IndexingService.OnDocumentRemoving(this, new RemoveEventArgs(itemId, namedIndex.Name));
			bool flag = this.DeleteFromIndex(namedIndex, itemId, removeRef);
			if (flag)
			{
				Niteco.Search.IndexingService.IndexingService.OnDocumentRemoved(this, new RemoveEventArgs(itemId, namedIndex.Name));
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End deleting document with id field: '{0}'", itemId));
			}
		}
		private void RemoveByVirtualPath(string virtualPath)
		{
			if (string.IsNullOrEmpty(virtualPath))
			{
				return;
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start removing all items under virtual path '{0}'.", virtualPath));
			var collection = new Collection<NamedIndex>();
			foreach (string current in IndexingServiceSettings.NamedIndexElements.Keys)
			{
				collection.Add(new NamedIndex(current));
			}
			int num = 0;
			Collection<ScoreDocument> scoreDocuments = this.GetScoreDocuments(string.Format("{0}:{1}*", IndexingServiceSettings.VirtualPathFieldName, virtualPath), false, collection, 0, IndexingServiceSettings.MaxHitsForReferenceSearch, IndexingServiceSettings.MaxHitsForReferenceSearch, null, out num);
			foreach (ScoreDocument current2 in scoreDocuments)
			{
				Document document = current2.Document;
				NamedIndex namedIndex = new NamedIndex(document.Get(IndexingServiceSettings.NamedIndexFieldName));
                string itemId = document.Get(IndexingServiceSettings.IdFieldName);
				this.Remove(itemId, namedIndex, true);
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End removing by virtual path.", new object[0]));
		}
		private void UpdateVirtualPaths(string oldVirtualPath, string newVirtualPath)
		{
			if (string.IsNullOrEmpty(newVirtualPath) || newVirtualPath.Equals(oldVirtualPath, System.StringComparison.InvariantCulture))
			{
				return;
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start updating virtual paths from old path: '{0}' to new path '{1}'", oldVirtualPath, newVirtualPath));
			var collection = new Collection<NamedIndex>();
			foreach (string current in IndexingServiceSettings.NamedIndexElements.Keys)
			{
				collection.Add(new NamedIndex(current));
			}
			int num = 0;
			var scoreDocuments = this.GetScoreDocuments(string.Format("{0}:{1}*", IndexingServiceSettings.VirtualPathFieldName, oldVirtualPath), false, collection, 0, IndexingServiceSettings.MaxHitsForReferenceSearch, IndexingServiceSettings.MaxHitsForReferenceSearch, null, out num);
			foreach (ScoreDocument current2 in scoreDocuments)
			{
				Document document = current2.Document;
				NamedIndex namedIndex = new NamedIndex(document.Get(IndexingServiceSettings.NamedIndexFieldName));
                string text = document.Get(IndexingServiceSettings.IdFieldName);
				string text2 = document.Get(IndexingServiceSettings.VirtualPathFieldName);
				text2 = text2.Remove(0, oldVirtualPath.Length);
				text2 = text2.Insert(0, newVirtualPath);
				document.RemoveField(IndexingServiceSettings.VirtualPathFieldName);
				document.Add(new Field(IndexingServiceSettings.VirtualPathFieldName, text2, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.VirtualPathFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.VirtualPathFieldName].FieldIndex));
				this.AddAllSearchableContentsFieldToDocument(document, namedIndex);
				this.Remove(text, namedIndex, false);
				this.WriteToIndex(text, document, namedIndex);
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Updated virtual path for document with id: '{0}'.", text));
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End updating virtual paths", new object[0]));
		}
		private void AddAllSearchableContentsFieldToDocument(Document doc, NamedIndex namedIndex)
		{
            string referenceId = doc.Get(IndexingServiceSettings.IdFieldName);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(doc.Get(IndexingServiceSettings.TitleFieldName));
			stringBuilder.Append(" ");
			stringBuilder.Append(doc.Get(IndexingServiceSettings.DisplayTextFieldName));
			stringBuilder.Append(" ");
			stringBuilder.Append(doc.Get(IndexingServiceSettings.MetadataFieldName));
			stringBuilder.Append(" ");
			stringBuilder.Append(this.GetReferenceData(referenceId, namedIndex));
			doc.RemoveField(IndexingServiceSettings.DefaultFieldName);
            doc.Add(new Field(IndexingServiceSettings.DefaultFieldName, stringBuilder.ToString(), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.DefaultFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.DefaultFieldName].FieldIndex));
		}
		private Document GetDocumentFromSyndicationItem(SyndicationItem syndicationItem, NamedIndex namedIndex)
		{
			string id = syndicationItem.Id;
			string value = IndexingServiceHandler.PrepareAuthors(syndicationItem);
			string value2 = (syndicationItem.Title != null) ? syndicationItem.Title.Text : "";
			string displayText = (syndicationItem.Content != null) ? ((TextSyndicationContent)syndicationItem.Content).Text : "";
			System.DateTime dateTime = (syndicationItem.PublishDate.DateTime.Year < 2) ? System.DateTime.Now : syndicationItem.PublishDate.DateTime;
			System.DateTime dateTime2 = (syndicationItem.LastUpdatedTime.DateTime.Year < 2) ? System.DateTime.Now : syndicationItem.LastUpdatedTime.DateTime;
			string value3 = (syndicationItem.BaseUri != null) ? syndicationItem.BaseUri.ToString() : "";
			string attributeValue = IndexingServiceHandler.GetAttributeValue(syndicationItem, "BoostFactor");
			string attributeValue2 = IndexingServiceHandler.GetAttributeValue(syndicationItem, "Culture");
			string attributeValue3 = IndexingServiceHandler.GetAttributeValue(syndicationItem, "Type");
			string attributeValue4 = IndexingServiceHandler.GetAttributeValue(syndicationItem, "ReferenceId");
			string elementValue = IndexingServiceHandler.GetElementValue(syndicationItem, "Metadata");
			string attributeValue5 = IndexingServiceHandler.GetAttributeValue(syndicationItem, "ItemStatus");
			bool flag = false;
			System.DateTime dateTime3;
			if (System.DateTime.TryParse(IndexingServiceHandler.GetAttributeValue(syndicationItem, "PublicationEnd"), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime3))
			{
				flag = true;
			}
			bool flag2 = false;
			System.DateTime dateTime4;
			if (System.DateTime.TryParse(IndexingServiceHandler.GetAttributeValue(syndicationItem, "PublicationStart"), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime4))
			{
				flag2 = true;
			}
			CategoriesFieldStoreSerializer categoriesFieldStoreSerializer = new CategoriesFieldStoreSerializer(syndicationItem);
			AclFieldStoreSerializer aclFieldStoreSerializer = new AclFieldStoreSerializer(syndicationItem);
			VirtualPathFieldStoreSerializer virtualPathFieldStoreSerializer = new VirtualPathFieldStoreSerializer(syndicationItem);
			AuthorsFieldStoreSerializer authorsFieldStoreSerializer = new AuthorsFieldStoreSerializer(syndicationItem);
			string empty = string.Empty;
			string empty2 = string.Empty;
			IndexingServiceHandler.SplitDisplayTextToMetadata(displayText, elementValue, out empty, out empty2);
			Document document = new Document();
            document.Add(new Field(IndexingServiceSettings.IdFieldName, id, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.IdFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.IdFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.TitleFieldName, value2, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.TitleFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.TitleFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.DisplayTextFieldName, empty, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.DisplayTextFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.DisplayTextFieldName].FieldIndex));
            document.Add(new Field(IndexingServiceSettings.CreatedFieldName, Regex.Replace(dateTime.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", ""), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CreatedFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CreatedFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.ModifiedFieldName, Regex.Replace(dateTime2.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", ""), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ModifiedFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ModifiedFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.PublicationEndFieldName, flag ? Regex.Replace(dateTime3.ToUniversalTime().ToString("u"), "\\D", "") : "no", IndexingServiceSettings.FieldProperties[IndexingServiceSettings.PublicationEndFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.PublicationEndFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.PublicationStartFieldName, flag2 ? Regex.Replace(dateTime4.ToUniversalTime().ToString("u"), "\\D", "") : "no", IndexingServiceSettings.FieldProperties[IndexingServiceSettings.PublicationStartFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.PublicationStartFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.UriFieldName, value3, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.UriFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.UriFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.MetadataFieldName, empty2, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.MetadataFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.MetadataFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.CategoriesFieldName, categoriesFieldStoreSerializer.ToFieldStoreValue(), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CategoriesFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CategoriesFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.CultureFieldName, attributeValue2, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CultureFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.CultureFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.AuthorsFieldName, value, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AuthorsFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AuthorsFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.TypeFieldName, attributeValue3, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.TypeFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.TypeFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.ReferenceIdFieldName, attributeValue4, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ReferenceIdFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ReferenceIdFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.AclFieldName, aclFieldStoreSerializer.ToFieldStoreValue(), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AclFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AclFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.VirtualPathFieldName, virtualPathFieldStoreSerializer.ToFieldStoreValue(), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.VirtualPathFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.VirtualPathFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.AuthorStorageFieldName, authorsFieldStoreSerializer.ToFieldStoreValue(), IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AuthorStorageFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.AuthorStorageFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.NamedIndexFieldName, namedIndex.Name, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.NamedIndexFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.NamedIndexFieldName].FieldIndex));
			document.Add(new Field(IndexingServiceSettings.ItemStatusFieldName, attributeValue5, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ItemStatusFieldName].FieldStore, IndexingServiceSettings.FieldProperties[IndexingServiceSettings.ItemStatusFieldName].FieldIndex));
			this.AddAllSearchableContentsFieldToDocument(document, namedIndex);
			float num = 1f;
			document.Boost = (float.TryParse(attributeValue, out num) ? num : 1f);
			return document;
		}
		private string GetReferenceData(string referenceId, NamedIndex namedIndex)
		{
			if (namedIndex.ReferenceDirectory == null)
			{
				return string.Empty;
			}
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			try
			{
				namedIndex = new NamedIndex(namedIndex.Name, true);
				int num = 0;
				Collection<ScoreDocument> collection = this.SingleIndexSearch(string.Format("{0}:{1}", IndexingServiceSettings.ReferenceIdFieldName, QueryParser.Escape(referenceId)), namedIndex, IndexingServiceSettings.MaxHitsForReferenceSearch, null, out num);
				foreach (ScoreDocument current in collection)
				{
					Document document = current.Document;
					stringBuilder.Append(document.Get(IndexingServiceSettings.TitleFieldName));
					stringBuilder.Append(" ");
					stringBuilder.Append(document.Get(IndexingServiceSettings.DisplayTextFieldName));
					stringBuilder.Append(" ");
					stringBuilder.Append(document.Get(IndexingServiceSettings.MetadataFieldName));
					stringBuilder.Append(" ");
				}
			}
			catch (System.Exception ex)
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Could not get reference data for id: {0}. Message: {1}{2}{3}", new object[]
				{
					referenceId,
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
				return null;
			}
			return stringBuilder.ToString();
		}
		private SyndicationItem GetSyndicationItemFromDocument(ScoreDocument scoreDocument)
		{
			Document document = scoreDocument.Document;
			NamedIndex namedIndex = new NamedIndex(document.Get(IndexingServiceSettings.NamedIndexFieldName));
			SyndicationItem syndicationItem = new SyndicationItem();
            syndicationItem.Id = (namedIndex.IncludeInResponse(IndexingServiceSettings.IdFieldName) ? document.Get(IndexingServiceSettings.IdFieldName) : "");
			syndicationItem.Title = (namedIndex.IncludeInResponse(IndexingServiceSettings.TitleFieldName) ? new TextSyndicationContent(document.Get(IndexingServiceSettings.TitleFieldName)) : new TextSyndicationContent(""));
			syndicationItem.Content = (namedIndex.IncludeInResponse(IndexingServiceSettings.DisplayTextFieldName) ? new TextSyndicationContent(document.Get(IndexingServiceSettings.DisplayTextFieldName)) : new TextSyndicationContent(""));
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.ModifiedFieldName))
			{
				syndicationItem.LastUpdatedTime = new System.DateTimeOffset(System.Convert.ToDateTime(Regex.Replace(document.Get(IndexingServiceSettings.ModifiedFieldName), "(\\d{4})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})", "$1-$2-$3 $4:$5:$6"), System.Globalization.CultureInfo.InvariantCulture));
			}
            if (namedIndex.IncludeInResponse(IndexingServiceSettings.CreatedFieldName))
			{
                syndicationItem.PublishDate = new System.DateTimeOffset(System.Convert.ToDateTime(Regex.Replace(document.Get(IndexingServiceSettings.CreatedFieldName), "(\\d{4})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})", "$1-$2-$3 $4:$5:$6"), System.Globalization.CultureInfo.InvariantCulture));
			}
			Uri uri;
			if (Uri.TryCreate(document.Get(IndexingServiceSettings.UriFieldName), UriKind.RelativeOrAbsolute, out uri))
			{
				syndicationItem.BaseUri = (namedIndex.IncludeInResponse(IndexingServiceSettings.UriFieldName) ? uri : null);
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.CultureFieldName))
			{
                syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("Culture", IndexingServiceConstants.Namespace), document.Get(IndexingServiceSettings.CultureFieldName));
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.ItemStatusFieldName))
			{
                syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("ItemStatus", IndexingServiceConstants.Namespace), document.Get(IndexingServiceSettings.ItemStatusFieldName));
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.TypeFieldName))
			{
                syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("Type", IndexingServiceConstants.Namespace), document.Get(IndexingServiceSettings.TypeFieldName));
			}
            syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("Score", IndexingServiceConstants.Namespace), scoreDocument.Score.ToString(System.Globalization.CultureInfo.InvariantCulture));
            syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("DataUri", IndexingServiceConstants.Namespace), document.Get("DataUri"));
            syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("BoostFactor", IndexingServiceConstants.Namespace), document.Boost.ToString(System.Globalization.CultureInfo.InvariantCulture));
            syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("NamedIndex", IndexingServiceConstants.Namespace), document.Get(IndexingServiceSettings.NamedIndexFieldName));
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.PublicationEndFieldName))
			{
				string text = document.Get(IndexingServiceSettings.PublicationEndFieldName);
				if (!text.Equals("no"))
				{
                    syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("PublicationEnd", IndexingServiceConstants.Namespace), System.Convert.ToDateTime(Regex.Replace(text, "(\\d{4})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})", "$1-$2-$3 $4:$5:$6Z"), System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));
				}
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.PublicationStartFieldName))
			{
				string text2 = document.Get(IndexingServiceSettings.PublicationStartFieldName);
				if (!text2.Equals("no"))
				{
                    syndicationItem.AttributeExtensions.Add(new XmlQualifiedName("PublicationStart", IndexingServiceConstants.Namespace), System.Convert.ToDateTime(Regex.Replace(text2, "(\\d{4})(\\d{2})(\\d{2})(\\d{2})(\\d{2})(\\d{2})", "$1-$2-$3 $4:$5:$6Z"), System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));
				}
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.MetadataFieldName))
			{
                syndicationItem.ElementExtensions.Add(new SyndicationElementExtension("Metadata", IndexingServiceConstants.Namespace, document.Get(IndexingServiceSettings.MetadataFieldName)));
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.CategoriesFieldName))
			{
				string indexFieldStoreValue = document.Get(IndexingServiceSettings.CategoriesFieldName);
				new CategoriesFieldStoreSerializer(indexFieldStoreValue).AddFieldStoreValueToSyndicationItem(syndicationItem);
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.AuthorsFieldName))
			{
				string indexFieldStoreValue2 = document.Get(IndexingServiceSettings.AuthorStorageFieldName);
				new AuthorsFieldStoreSerializer(indexFieldStoreValue2).AddFieldStoreValueToSyndicationItem(syndicationItem);
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.AclFieldName))
			{
				string indexFieldStoreValue3 = document.Get(IndexingServiceSettings.AclFieldName);
				new AclFieldStoreSerializer(indexFieldStoreValue3).AddFieldStoreValueToSyndicationItem(syndicationItem);
			}
			if (namedIndex.IncludeInResponse(IndexingServiceSettings.VirtualPathFieldName))
			{
				string indexFieldStoreValue4 = document.Get(IndexingServiceSettings.VirtualPathFieldName);
				new VirtualPathFieldStoreSerializer(indexFieldStoreValue4).AddFieldStoreValueToSyndicationItem(syndicationItem);
			}
			return syndicationItem;
		}
		private static bool IsValidIndex(string namedIndexName)
		{
			if (string.IsNullOrEmpty(namedIndexName))
			{
				namedIndexName = IndexingServiceSettings.DefaultIndexName;
				if (IndexingServiceSettings.NamedIndexDirectories.ContainsKey(namedIndexName))
				{
					return true;
				}
			}
			else
			{
				if (IndexingServiceSettings.NamedIndexDirectories.ContainsKey(namedIndexName))
				{
					return true;
				}
			}
			IndexingServiceSettings.HandleServiceError(string.Format("Named index \"{0}\" is not valid, it does not exist in configuration or has faulty configuration", namedIndexName));
			return false;
		}
		private Document GetDocumentById(string id, NamedIndex namedIndex)
		{
			int num = 0;
            Collection<ScoreDocument> collection = this.SingleIndexSearch(string.Format("{0}:{1}", IndexingServiceSettings.IdFieldName, QueryParser.Escape(id)), namedIndex, 1, null, out num);
			if (collection.Count > 0)
			{
				return collection[0].Document;
			}
			return null;
		}
		private bool DeleteFromIndex(NamedIndex namedIndex, string itemId, bool deleteRef)
		{
			ReaderWriterLockSlim readerWriterLockSlim = IndexingServiceSettings.ReaderWriterLocks[namedIndex.Name];
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start deleting Lucene document with id field '{0}' from index '{1}'", itemId, namedIndex.Name));
			int num = 0;
			int num2 = 0;
			readerWriterLockSlim.EnterWriteLock();
			try
			{
				using (IndexReader indexReader = IndexReader.Open(namedIndex.Directory, false))
				{
                    Term term = new Term(IndexingServiceSettings.IdFieldName, itemId);
					num = indexReader.DeleteDocuments(term);
					num2 = indexReader.NumDeletedDocs;
				}
			}
			catch (System.Exception ex)
			{
                IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("Failed to delete Document with id: {0}. Message: {1}{2}{3}", new object[]
				{
					itemId.ToString(),
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
				bool result = false;
				return result;
			}
			finally
			{
				readerWriterLockSlim.ExitWriteLock();
			}
			if (num == 0)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Failed to delete Document with id: {0}. Document does not exist.", itemId.ToString()));
				return false;
			}
			if (deleteRef && namedIndex.ReferenceDirectory != null)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start deleting reference documents for id '{0}'", itemId.ToString()));
				ReaderWriterLockSlim readerWriterLockSlim2 = IndexingServiceSettings.ReaderWriterLocks[namedIndex.ReferenceName];
				readerWriterLockSlim2.EnterWriteLock();
				try
				{
					using (IndexReader indexReader2 = IndexReader.Open(namedIndex.ReferenceDirectory, false))
					{
						Term term2 = new Term(IndexingServiceSettings.ReferenceIdFieldName, itemId);
						indexReader2.DeleteDocuments(term2);
					}
				}
				catch (System.Exception ex2)
				{
					IndexingServiceSettings.HandleServiceError(string.Format("Failed to delete referencing Documents for reference id: {0}. Message: {1}{2}{3}", new object[]
					{
						itemId.ToString(),
						ex2.Message,
						System.Environment.NewLine,
						ex2.StackTrace
					}));
					bool result = false;
					return result;
				}
				finally
				{
					readerWriterLockSlim2.ExitWriteLock();
				}
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End deleting reference documents for id '{0}'", itemId.ToString()));
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End deleting Lucene document with id field: '{0}'", itemId));
			if (namedIndex.PendingDeletesOptimizeThreshold > 0 && num2 >= namedIndex.PendingDeletesOptimizeThreshold)
			{
				this.OptimizeIndex(namedIndex);
			}
			return true;
		}
		private bool DocumentExists(string itemId, NamedIndex namedIndex)
		{
			try
			{
				if (this.GetDocumentById(itemId, namedIndex) != null)
				{
					return true;
				}
			}
			catch (System.Exception ex)
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Could not verify document existense for id: '{0}'. Message: {1}{2}{3}", new object[]
				{
					itemId,
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
			}
			return false;
		}
		private void WriteToIndex(string itemId, Document doc, NamedIndex namedIndex)
		{
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start writing document with id '{0}' to index '{1}' with analyzer '{2}'", itemId, namedIndex.Name, IndexingServiceSettings.Analyzer.ToString()));
			if (this.DocumentExists(itemId, namedIndex))
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Failed to write to index: '{0}'. Document with id: '{1}' already exists", namedIndex.Name, itemId));
				return;
			}
			ReaderWriterLockSlim readerWriterLockSlim = IndexingServiceSettings.ReaderWriterLocks[namedIndex.Name];
			readerWriterLockSlim.EnterWriteLock();
			try
			{
				using (IndexWriter indexWriter = new IndexWriter(namedIndex.Directory, IndexingServiceSettings.Analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					indexWriter.AddDocument(doc);
				}
			}
			catch (System.Exception ex)
			{
                IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("Failed to write to index: '{0}'. Message: {1}{2}{3}", new object[]
				{
					namedIndex.Name,
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
				return;
			}
			finally
			{
				readerWriterLockSlim.ExitWriteLock();
			}
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End writing to index", new object[0]));
		}
		private void OptimizeIndex(NamedIndex namedIndex)
		{
			ReaderWriterLockSlim readerWriterLockSlim = IndexingServiceSettings.ReaderWriterLocks[namedIndex.Name];
			readerWriterLockSlim.EnterWriteLock();
			try
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Start optimizing index", new object[0]));
				using (IndexWriter indexWriter = new IndexWriter(namedIndex.Directory, IndexingServiceSettings.Analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
				{
					indexWriter.Optimize();
				}
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("End optimizing index", new object[0]));
			}
			catch (System.Exception ex)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("Failed to optimize index: '{0}'. Message: {1}{2}{3}", new object[]
				{
					namedIndex.Name,
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
			}
			finally
			{
				readerWriterLockSlim.ExitWriteLock();
			}
			Niteco.Search.IndexingService.IndexingService.OnIndexedOptimized(this, new OptimizedEventArgs(namedIndex.Name));
			IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Optimized index: '{0}'", namedIndex.Name));
		}

        #region Customized
        // Hieu Le: Added sortFields param
        private Collection<ScoreDocument> SingleIndexSearch(string q, NamedIndex namedIndex, int maxHits, Collection<SortField> sortFields, out int totalHits)
		{
			Collection<ScoreDocument> collection = new Collection<ScoreDocument>();
			totalHits = 0;
			ReaderWriterLockSlim readerWriterLockSlim = IndexingServiceSettings.ReaderWriterLocks[namedIndex.Name];
			readerWriterLockSlim.EnterReadLock();
			try
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Creating Lucene QueryParser for index '{0}' with expression '{1}' with analyzer '{2}'", namedIndex.Name, q, IndexingServiceSettings.Analyzer.ToString()));
                QueryParser queryParser = new PerFieldQueryParserWrapper(IndexingServiceSettings.LuceneVersion, IndexingServiceSettings.DefaultFieldName, IndexingServiceSettings.Analyzer, IndexingServiceSettings.LowercaseFields);
				Query query = queryParser.Parse(q);

				using (IndexSearcher indexSearcher = new IndexSearcher(namedIndex.Directory, true))
				{
                    TopDocs topDocs = sortFields != null ? indexSearcher.Search(query, null, maxHits, new Sort(sortFields.ToArray())) : indexSearcher.Search(query, maxHits);
					totalHits = topDocs.TotalHits;
					ScoreDoc[] scoreDocs = topDocs.ScoreDocs;
					for (int i = 0; i < scoreDocs.Length; i++)
					{
						collection.Add(new ScoreDocument(indexSearcher.Doc(scoreDocs[i].Doc), scoreDocs[i].Score));
					}
				}
			}
			catch (System.Exception ex)
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Failed to search index '{0}'. Index seems to be corrupt! Message: {1}{2}{3}", new object[]
				{
					namedIndex.Name,
					ex.Message,
					System.Environment.NewLine,
					ex.StackTrace
				}));
			}
			finally
			{
				readerWriterLockSlim.ExitReadLock();
			}
			return collection;
		}

		private static Collection<ScoreDocument> MultiIndexSearch(string q, Collection<NamedIndex> namedIndexes, int maxHits, Collection<SortField> sortFields, out int totalHits)
		{
			Query[] array = new Query[namedIndexes.Count];
			var array2 = new IndexSearcher[namedIndexes.Count];
			var collection = new Collection<ReaderWriterLockSlim>();
			int num = 0;
			foreach (NamedIndex current in namedIndexes)
			{
				ReaderWriterLockSlim readerWriterLockSlim = IndexingServiceSettings.ReaderWriterLocks[current.Name];
				collection.Add(readerWriterLockSlim);
				readerWriterLockSlim.EnterReadLock();
				try
				{
					IndexingServiceSettings.IndexingServiceServiceLog.Debug(string.Format("Creating Lucene QueryParser for index '{0}' with expression '{1}' with analyzer '{2}'", current.Name, q, IndexingServiceSettings.Analyzer));
                    QueryParser queryParser = new PerFieldQueryParserWrapper(IndexingServiceSettings.LuceneVersion, IndexingServiceSettings.DefaultFieldName, IndexingServiceSettings.Analyzer, IndexingServiceSettings.LowercaseFields);
					array[num] = queryParser.Parse(q);
					array2[num] = new IndexSearcher(current.Directory, true);
				}
				catch (System.Exception ex)
				{
					IndexingServiceSettings.HandleServiceError(string.Format("Failed to create sub searcher for index '{0}' Message: {1}{2}{3}", new object[]
					{
						current.Name,
						ex.Message,
						System.Environment.NewLine,
						ex.StackTrace
					}));
				}
				finally
				{
					readerWriterLockSlim.ExitReadLock();
				}
				num++;
			}
			Query query = array[0].Combine(array);
			var collection2 = new Collection<ScoreDocument>();
			totalHits = 0;
			foreach (ReaderWriterLockSlim current2 in collection)
			{
				current2.EnterReadLock();
			}
			try
			{
				using (var multiSearcher = new MultiSearcher(array2))
				{
				    TopDocs topDocs = sortFields != null ? multiSearcher.Search(query, null, maxHits, new Sort(sortFields.ToArray())) : multiSearcher.Search(query, maxHits);
				    totalHits = topDocs.TotalHits;
					ScoreDoc[] scoreDocs = topDocs.ScoreDocs;
					for (int i = 0; i < scoreDocs.Length; i++)
					{
						collection2.Add(new ScoreDocument(multiSearcher.Doc(scoreDocs[i].Doc), scoreDocs[i].Score));
					}
				}
			}
			catch (System.Exception ex2)
			{
				IndexingServiceSettings.HandleServiceError(string.Format("Failed to get hits from MultiSearcher! Message: {0}{1}{2}", ex2.Message, System.Environment.NewLine, ex2.StackTrace));
			}
			finally
			{
				foreach (ReaderWriterLockSlim current3 in collection)
				{
					current3.ExitReadLock();
				}
			}
			return collection2;
		}
        #endregion

        private static bool IsModifyIndex(string namedIndexName)
		{
			if (string.IsNullOrEmpty(namedIndexName))
			{
				namedIndexName = IndexingServiceSettings.DefaultIndexName;
			}
			if (IndexingServiceSettings.NamedIndexElements[namedIndexName].ReadOnly)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("cannot modify index: '{0}'. Index is readonly.", namedIndexName));
				IndexingServiceSettings.SetResponseHeaderStatusCode(401);
				return false;
			}
			return true;
		}
		private static string PrepareExpression(string q, bool excludeNotPublished)
		{
			string text = IndexingServiceHandler.PrepareEscapeFields(q, IndexingServiceSettings.CategoriesFieldName);
			text = IndexingServiceHandler.PrepareEscapeFields(text, IndexingServiceSettings.AclFieldName);
			string arg = Regex.Replace(System.DateTime.Now.ToUniversalTime().ToString("u"), "\\D", "");
			if (excludeNotPublished)
			{
				text = string.Format("({0}) AND ({1}:(no) OR {1}:[{2} TO 99999999999999])", text, IndexingServiceSettings.PublicationEndFieldName, arg);
			}
			if (excludeNotPublished)
			{
				text = string.Format("({0}) AND ({1}:(no) OR {1}:[00000000000000 TO {2}])", text, IndexingServiceSettings.PublicationStartFieldName, arg);
			}
			return text;
		}
		private static string PrepareEscapeFields(string q, string fieldName)
		{
			MatchEvaluator evaluator = delegate(Match m)
			{
				if (m.Groups["fieldname"].Value.Equals(fieldName + ":"))
				{
					return string.Concat(new object[]
					{
						m.Groups["fieldname"],
						"\"[[",
						m.Groups["terms"].Value.Replace("(", "").Replace(")", ""),
						"]]\""
					});
				}
				return m.Groups[0].Value;
			};
			return Regex.Replace(q, "(?<fieldname>\\w+:)?(?:(?<terms>\\([^()]*\\))|(?<terms>[^\\s()\"]+)|(?<terms>\"[^\"]*\"))", evaluator);
		}
		private static string PrepareAuthors(SyndicationItem syndicationItem)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			if (syndicationItem.Authors != null)
			{
				foreach (SyndicationPerson current in syndicationItem.Authors)
				{
					stringBuilder.Append(current.Name);
					stringBuilder.Append(" ");
				}
			}
			return stringBuilder.ToString().Trim();
		}
		private static void SetElementValue(SyndicationItem item, string elementExtensionName, string value)
		{
            SyndicationElementExtension extension = new SyndicationElementExtension(elementExtensionName, IndexingServiceConstants.Namespace, value);
			int num = 0;
			foreach (SyndicationElementExtension current in item.ElementExtensions)
			{
                if (current.OuterName == elementExtensionName && current.OuterNamespace == IndexingServiceConstants.Namespace)
				{
					break;
				}
				num++;
			}
			item.ElementExtensions.RemoveAt(num);
			item.ElementExtensions.Add(extension);
		}
		private static void SetAttributeValue(SyndicationItem item, string attributeExtensionName, string value)
		{
            item.AttributeExtensions[new XmlQualifiedName(attributeExtensionName, IndexingServiceConstants.Namespace)] = value;
		}
		private static string GetAttributeValue(SyndicationItem syndicationItem, string attributeName)
		{
			string result = string.Empty;
            if (syndicationItem.AttributeExtensions.ContainsKey(new XmlQualifiedName(attributeName, IndexingServiceConstants.Namespace)))
			{
                result = syndicationItem.AttributeExtensions[new XmlQualifiedName(attributeName, IndexingServiceConstants.Namespace)];
			}
			return result;
		}
		private static string GetElementValue(SyndicationItem syndicationItem, string elementName)
		{
            Collection<string> collection = syndicationItem.ElementExtensions.ReadElementExtensions<string>(elementName, IndexingServiceConstants.Namespace);
			string result = "";
			if (collection.Count > 0)
			{
				result = syndicationItem.ElementExtensions.ReadElementExtensions<string>(elementName, IndexingServiceConstants.Namespace).ElementAt(0);
			}
			return result;
		}
		private void UpdateReference(string referenceId, string itemId, NamedIndex mainNamedIndex)
		{
			Document documentById = this.GetDocumentById(referenceId, mainNamedIndex);
			if (documentById == null)
			{
				IndexingServiceSettings.IndexingServiceServiceLog.Error(string.Format("Could not find main document with id: '{0}' for referencing item id '{1}'. Continuing anyway, index will heal when main document is added/updated.", referenceId, itemId));
				return;
			}
			this.AddAllSearchableContentsFieldToDocument(documentById, mainNamedIndex);
			this.Remove(referenceId, mainNamedIndex, false);
			this.WriteToIndex(referenceId, documentById, mainNamedIndex);
		}
		private static void SplitDisplayTextToMetadata(string displayText, string metadata, out string displayTextOut, out string metadataOut)
		{
			displayTextOut = string.Empty;
			metadataOut = string.Empty;
			if (displayText.Length <= IndexingServiceSettings.MaxDisplayTextLength)
			{
				displayTextOut = displayText;
				metadataOut = metadata;
				return;
			}
			displayTextOut = displayText.Substring(0, IndexingServiceSettings.MaxDisplayTextLength);
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			stringBuilder.Append(metadata);
			stringBuilder.Append(" ");
			stringBuilder.Append(displayText.Substring(IndexingServiceSettings.MaxDisplayTextLength, displayText.Length - IndexingServiceSettings.MaxDisplayTextLength));
			metadataOut = stringBuilder.ToString();
		}
		private static string GetFileText(string path)
		{
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
			IFilter filter = null;
			object pUnkOuter = null;
			int num = TextFilter.LoadIFilter(path, pUnkOuter, ref filter);
			if (num != 0)
			{
				return null;
			}
			int cAttributes = 0;
			IFILTER_FLAGS iFILTER_FLAGS = (IFILTER_FLAGS)0;
			IFilterReturnCodes filterReturnCodes = filter.Init(IFILTER_INIT.IFILTER_INIT_CANON_SPACES | IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES | IFILTER_INIT.IFILTER_INIT_APPLY_CRAWL_ATTRIBUTES | IFILTER_INIT.IFILTER_INIT_INDEXING_ONLY, cAttributes, System.IntPtr.Zero, ref iFILTER_FLAGS);
			if (filterReturnCodes != IFilterReturnCodes.S_OK)
			{
				throw new System.Exception(string.Format("IFilter initialisation failed: {0}", System.Enum.GetName(filterReturnCodes.GetType(), filterReturnCodes)));
			}
			while (filterReturnCodes == IFilterReturnCodes.S_OK)
			{
				STAT_CHUNK sTAT_CHUNK = default(STAT_CHUNK);
				filterReturnCodes = filter.GetChunk(ref sTAT_CHUNK);
				if (filterReturnCodes == IFilterReturnCodes.S_OK && sTAT_CHUNK.flags == CHUNKSTATE.CHUNK_TEXT)
				{
					if (stringBuilder.Length > 0 && sTAT_CHUNK.breakType != CHUNK_BREAKTYPE.CHUNK_NO_BREAK)
					{
						stringBuilder.AppendLine();
					}
					int num2 = 65536;
					IFilterReturnCodes filterReturnCodes2 = IFilterReturnCodes.S_OK;
					System.Text.StringBuilder stringBuilder2 = new System.Text.StringBuilder(num2);
					while (filterReturnCodes2 == IFilterReturnCodes.S_OK)
					{
						filterReturnCodes2 = filter.GetText(ref num2, stringBuilder2);
						if ((filterReturnCodes2 == IFilterReturnCodes.S_OK || filterReturnCodes2 == IFilterReturnCodes.FILTER_S_LAST_TEXT) && num2 > 0)
						{
							stringBuilder.Append(stringBuilder2.ToString(0, (num2 > stringBuilder2.Length) ? stringBuilder2.Length : num2));
						}
						if (filterReturnCodes2 == IFilterReturnCodes.FILTER_S_LAST_TEXT)
						{
							break;
						}
						num2 = 65536;
					}
				}
			}
			System.Runtime.InteropServices.Marshal.ReleaseComObject(filter);
			return stringBuilder.ToString();
		}
	}
}
