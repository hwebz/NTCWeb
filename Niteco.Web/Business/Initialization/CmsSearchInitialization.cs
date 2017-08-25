using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAccess;
using EPiServer.SpecializedProperties;
using Niteco.ContentTypes.Pages;
using Niteco.Search.IndexingService;
using EPiServer.ServiceLocation;
using log4net;
using Lucene.Net.Documents;
using Niteco.Common.Extensions;
using Niteco.Common.Helpers;
using Niteco.Search;
using Niteco.Search.Extensions;
using Niteco.Search.Fields;
using AccessLevel = EPiServer.Security.AccessLevel;
using Niteco.ContentTypes.Blocks;
using EPiServer.Core.Html.StringParsing;
using Niteco.ContentTypes.Extension;

namespace Niteco.Web.Business.Initialization
{
	/// <summary>
	/// Initializes the CMS Search Client Facade
	/// </summary>
	[EPiServer.Framework.ModuleDependencyAttribute(typeof(EPiServer.Web.InitializationModule))]
	internal class CmsSearchInitialization : EPiServer.Framework.IInitializableModule
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(CmsSearchInitialization));

		#region IInitializableModule Members

		/// <summary>
		/// Registers handlers for page and file events to allow for indexing.
		/// An initial index of pages and files is also done if required.
		/// </summary>
		/// <param name="context"></param>
		public void Initialize(EPiServer.Framework.Initialization.InitializationEngine context)
		{
			if (context.HostType == EPiServer.Framework.Initialization.HostType.WebApplication)
			{
				DataFactory.Instance.MovedContent += OnMovedContent;
				DataFactory.Instance.DeletedContent += OnDeletedContent;
				DataFactory.Instance.PublishedContent += OnPublishedContent;
				DataFactory.Instance.PublishingContent += OnPublishingContent;
			}

			IndexingService.DocumentAdding += IndexingServiceOnDocumentAdding;
			//IndexingService.DocumentRemoving += IndexingService_DocumentRemoving;
		}



		public void Preload(string[] parameters)
		{
		}

		public void Uninitialize(EPiServer.Framework.Initialization.InitializationEngine context)
		{
		}

		#endregion

		private void IndexingServiceOnDocumentAdding(object sender, EventArgs eventArgs)
		{
			var addUpdateEventArgs = eventArgs as AddUpdateEventArgs;

			if (addUpdateEventArgs == null)
			{
				return;
			}

			var document = addUpdateEventArgs.Document;

			if (document.IsUnifiedFileDocument())
			{
				return;
			}

			var content = document.GetContent<IContent>();

			var page = content as PageData;

			if (page == null)
			{
				return;
			}


			var sortIndexField = new SortIndexField();
			document.Add(new Field(sortIndexField.Name, page.GetPropertyValue<int>(PropertyName.Internal.PagePeerOrder).ToString(CultureInfo.InvariantCulture), Field.Store.NO, Field.Index.ANALYZED));

			var startPublishedField = new StartPublishedField();
			document.Add(new Field(startPublishedField.Name, Regex.Replace(page.StartPublish.ToString("u", System.Globalization.CultureInfo.InvariantCulture), "\\D", ""), Field.Store.NO, Field.Index.ANALYZED));

			// Hieu Le: Add event tag to search index
			//if (content.ContentTypeID == ContentTypes.EventPageTypeId)
			//{
			//    var eventPage = content as EventPageData;
			//    if (eventPage != null)
			//    {
			//        if (!string.IsNullOrWhiteSpace(eventPage.Tag))
			//        {
			//            var tags = eventPage.Tag.Trim().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			//            if (tags.Length > 0)
			//            {
			//                MD5 md5 = System.Security.Cryptography.MD5.Create();
			//                var sb = new StringBuilder();
			//                foreach (var tag in tags)
			//                {
			//                    sb.Append("[[");
			//                    foreach (var b in md5.ComputeHash(Encoding.ASCII.GetBytes(tag.ToLowerInvariant().Trim())))
			//                    {
			//                        sb.Append(b.ToString("X2"));
			//                    }
			//                    sb.Append("]] ");
			//                }
			//                var eventTagField = new EventTagField();
			//                document.Add(new Field(eventTagField.Name, sb.ToString(), Field.Store.YES, Field.Index.ANALYZED));
			//            }
			//        }

			//        var eventDateField = new EventDateField();
			//        document.Add(new Field(eventDateField.Name, Regex.Replace(eventPage.Date.ToString("u", CultureInfo.InvariantCulture), "\\D", ""), Field.Store.YES, Field.Index.ANALYZED));


			//    }
			//}

			//h.t indexing advertiser
			//h.t indexing industry
			//if (content is CasePageData)
			//{
			//    var casePageData = content as CasePageData;
			//    var caseAdvertiser = new CaseAdvertiserField();
			//    var caseIndustry = new CaseIndustryField();

			//    document.Add(new Field(caseAdvertiser.Name, "[[" + casePageData.Advertiser.ToString(CultureInfo.InvariantCulture) + "]] ", Field.Store.YES, Field.Index.ANALYZED));
			//    document.Add(new Field(caseIndustry.Name, "[[" + casePageData.Industry.ToString(CultureInfo.InvariantCulture) + "]] ", Field.Store.YES, Field.Index.ANALYZED));
			//}

			//if (content is ProductPageData)
			//{
			//    var productPageData = content as ProductPageData;
			//    var product = new ProductField();

			//    document.Add(new Field(product.Name, productPageData.Title.ToString(CultureInfo.InvariantCulture), Field.Store.YES, Field.Index.ANALYZED));
			//}
		}

		private void OnPublishingContent(object sender, ContentEventArgs contentEventArgs)
		{
			try
			{
				var contentSearchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

				if (contentEventArgs.Content is AboutUsPage)
				{

					AboutUsPage page = contentEventArgs.Content as AboutUsPage;
					if (page == null)
					{
						return;
					}

					ContentArea contentArea = page.MainContentArea;

					if (contentArea == null)
					{
						return;
					}

					StringBuilder stringBuilder = new StringBuilder();

					foreach (ContentAreaItem contentAreaItem in contentArea.Items)
					{
						IContent blockData = contentAreaItem.GetContent();

						IEnumerable<string> props = GetSearchablePropertyValues(blockData, blockData.ContentTypeID);

						stringBuilder.AppendFormat(" {0}", string.Join(" ", props));
					}

					var clone = page.CreateWritableClone() as AboutUsPage;

					clone.SearchText = stringBuilder.ToString();
					var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
					contentRepository.Save(clone, SaveAction.Save, AccessLevel.Read);
				}
				//include the block fragment text in the search text field
				if (contentEventArgs.Content is BlogDetailPage)
				{
					var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
					var xhtmlString = (contentEventArgs.Content as BlogDetailPage).Content;
					var fragments = xhtmlString.Fragments;
					var searchText = " ";
//					foreach (var fragment in fragments)
//					{
//						if (fragment is StaticFragment)
//						{
//							var staticFragment = fragment as StaticFragment;
//							searchText += staticFragment.InternalFormat + " ";
//						}
//						else if (fragment is ContentFragment)
//						{
//							var contentFragment = fragment as ContentFragment;
//							var referencedContent = contentRepository.Get<IContent>(contentFragment.ContentLink);
//							if (referencedContent is SiteBlockData)
//							{
//								var _searchText = (referencedContent as SiteBlockData).SearchText;
//								if (!string.IsNullOrWhiteSpace(_searchText))
//								{
//									searchText += _searchText + " ";
//								}
//							}
//							else
//							{
//								searchText += contentFragment.InternalFormat + " ";
//							}
//						}
//					}
					var clone = (contentEventArgs.Content as BlogDetailPage).CreateWritableClone() as BlogDetailPage;
//					clone.SearchText = searchText;
				    clone.SearchText = xhtmlString.ToBlocksIncludedString();
					contentRepository.Save(clone, SaveAction.Save, AccessLevel.Read);
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
				{
					log.Error(ex);
				}
			}
		}

		private void OnPublishedContent(object sender, ContentEventArgs contentEventArgs)
		{
			try
			{
				var contentSearchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();
				contentSearchHandler.UpdateItem(contentEventArgs.Content);

			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
				{
					log.Error(ex);
				}
			}
		}

		private void OnDeletedContent(object sender, DeleteContentEventArgs contentEventArgs)
		{
			try
			{
				var contentSearchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();
				contentSearchHandler.UpdateItem(contentEventArgs.Content);
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
				{
					log.Error(ex);
				}
			}
		}

		private void OnMovedContent(object sender, ContentEventArgs contentEventArgs)
		{
			try
			{
				var contentSearchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();
				var contenLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
				var content = contenLoader.Get<IContent>(contentEventArgs.ContentLink);
				var pageData = content as PageData;
				if (pageData == null || pageData.IsPublished())
				{
					contentSearchHandler.UpdateItem(content);
				}
			}
			catch (Exception ex)
			{
				if (log.IsErrorEnabled)
				{
					log.Error(ex);
				}
			}
		}

		private IEnumerable<string> GetSearchablePropertyValues(IContentData contentData, ContentType contentType)
		{
			if (contentType == null)
			{
				yield break;
			}

			foreach (PropertyDefinition current in
				from d in contentType.PropertyDefinitions
				where d.Searchable || typeof(IPropertyBlock).IsAssignableFrom(d.Type.DefinitionType)
				select d)
			{
				PropertyData propertyData = contentData.Property[current.Name];
				IPropertyBlock propertyBlock = propertyData as IPropertyBlock;
				if (propertyBlock != null)
				{
					foreach (
						string current2 in
							this.GetSearchablePropertyValues(
								propertyBlock.Block,
								propertyBlock.BlockPropertyDefinitionTypeID))
					{
						yield return current2;
					}
				}
				else
				{
					yield return propertyData.ToWebString();
				}
			}

			yield break;
		}

		private IEnumerable<string> GetSearchablePropertyValues(IContentData contentData, int contentTypeID)
		{
			IContentTypeRepository contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
			return this.GetSearchablePropertyValues(contentData, contentTypeRepository.Load(contentTypeID));
		}

	}
}
