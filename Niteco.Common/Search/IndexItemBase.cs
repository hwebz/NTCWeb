using System.Collections.ObjectModel;
using System.Text;
using EPiServer.HtmlParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;
using System.Xml.Linq;
namespace Niteco.Common.Search
{
	public abstract class IndexItemBase
	{
		private readonly SyndicationItem syndicationItem;
		private Collection<string> categories = new Collection<string>();
		private Collection<string> authors = new Collection<string>();
		private Collection<string> accessControlList = new Collection<string>();
		private Collection<string> virtualPathNodes = new Collection<string>();
		private string metadata;

	    public string Id
	    {
	        get { return this.SyndicationItem.Id; }
	        set { this.SyndicationItem.Id = value; }
	    }

	    public DateTime Created
	    {
	        get { return this.SyndicationItem.PublishDate.DateTime; }
	        set
	        {
	            if (value != DateTime.MinValue)
	            {
	                this.SyndicationItem.PublishDate = new DateTimeOffset(value);
	            }
	        }
	    }

	    public string Title
		{
			get
			{
				if (this.SyndicationItem.Title != null)
				{
					return this.SyndicationItem.Title.Text;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					this.SyndicationItem.Title = new TextSyndicationContent(SearchSettings.Config.HtmlStripTitle ? IndexItemBase.StripHtml(value) : value);
					return;
				}
				this.SyndicationItem.Title = null;
			}
		}

		public string DisplayText
		{
			get
			{
				if (this.SyndicationItem.Content != null)
				{
					return ((TextSyndicationContent)this.SyndicationItem.Content).Text;
				}
				return null;
			}
			set
			{
				if (value != null)
				{
					this.SyndicationItem.Content = new TextSyndicationContent(SearchSettings.Config.HtmlStripDisplayText ? IndexItemBase.StripHtml(value) : value);
					return;
				}
				this.SyndicationItem.Content = null;
			}
		}

		public DateTime Modified
		{
			get
			{
				return this.SyndicationItem.LastUpdatedTime.DateTime;
			}
			set
			{
				if (value != DateTime.MinValue)
				{
					this.SyndicationItem.LastUpdatedTime = new DateTimeOffset(value);
				}
			}
		}

	    public DateTime? PublicationEnd { get; set; }

	    public DateTime? PublicationStart { get; set; }

	    public ItemStatus ItemStatus { get; set; }

	    public string Metadata
	    {
	        get { return this.metadata; }
	        set
	        {
	            if (value != null)
	            {
	                this.metadata = (SearchSettings.Config.HtmlStripMetadata ? IndexItemBase.StripHtml(value) : value);
	                return;
	            }
	            this.metadata = null;
	        }
	    }

	    public string ItemType { get; set; }

	    public string Culture { get; set; }

	    public float BoostFactor { get; set; }

	    public string NamedIndex { get; set; }

	    public Uri Uri
	    {
	        get { return this.SyndicationItem.BaseUri; }
	        set { this.SyndicationItem.BaseUri = value; }
	    }

	    public Collection<string> Categories
	    {
	        get { return this.categories; }
	    }

	    public Collection<string> Authors
	    {
	        get { return this.authors; }
	    }

	    public Collection<string> AccessControlList
	    {
	        get { return this.accessControlList; }
	    }

	    public string ReferenceId { get; set; }

	    public Uri DataUri { get; set; }

	    public Collection<string> VirtualPathNodes
	    {
	        get { return this.virtualPathNodes; }
	    }

	    protected SyndicationItem SyndicationItem
	    {
	        get { return this.syndicationItem; }
	    }

	    protected IndexItemBase(string id)
		{
			this.syndicationItem = new SyndicationItem();
			this.Id = id;
			this.BoostFactor = 1f;
			this.Created = DateTime.Now;
			this.Modified = DateTime.Now;
			this.Title = string.Empty;
			this.DisplayText = string.Empty;
			this.Metadata = string.Empty;
			this.ItemType = string.Empty;
			this.Culture = string.Empty;
			this.NamedIndex = string.Empty;
			this.ItemStatus = ItemStatus.Approved;
		}

		protected internal virtual string ToSyndicationItemXml()
		{
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameBoostFactor, SearchSettings.Config.XmlQualifiedNamespace), this.BoostFactor.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameType, SearchSettings.Config.XmlQualifiedNamespace), this.ItemType);
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameCulture, SearchSettings.Config.XmlQualifiedNamespace), this.Culture);
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameNamedIndex, SearchSettings.Config.XmlQualifiedNamespace), this.NamedIndex);
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameReferenceId, SearchSettings.Config.XmlQualifiedNamespace), this.ReferenceId);
			this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameItemStatus, SearchSettings.Config.XmlQualifiedNamespace), ((int)this.ItemStatus).ToString(System.Globalization.CultureInfo.InvariantCulture));
			if (this.PublicationEnd.HasValue)
			{
				this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNamePublicationEnd, SearchSettings.Config.XmlQualifiedNamespace), this.PublicationEnd.Value.ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));
			}
			if (this.PublicationStart.HasValue)
			{
				this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNamePublicationStart, SearchSettings.Config.XmlQualifiedNamespace), this.PublicationStart.Value.ToUniversalTime().ToString("u", System.Globalization.CultureInfo.InvariantCulture));
			}
			if (this.DataUri != null)
			{
				this.SyndicationItem.AttributeExtensions.Add(new XmlQualifiedName(SearchSettings.Config.SyndicationItemAttributeNameDataUri, SearchSettings.Config.XmlQualifiedNamespace), this.DataUri.ToString());
			}
			this.SyndicationItem.ElementExtensions.Add(new SyndicationElementExtension(SearchSettings.Config.SyndicationItemElementNameMetadata, SearchSettings.Config.XmlQualifiedNamespace, this.Metadata));
			foreach (string current in 
				from c in this.Categories
				where !string.IsNullOrEmpty(c)
				select c)
			{
				this.SyndicationItem.Categories.Add(new SyndicationCategory(current));
			}
			foreach (string current2 in 
				from a in this.Authors
				where !string.IsNullOrEmpty(a)
				select a)
			{
				this.SyndicationItem.Authors.Add(new SyndicationPerson("", current2, ""));
			}
			XNamespace ns = SearchSettings.Config.XmlQualifiedNamespace;
			XElement xElement = new XElement(ns + "ACL");
			foreach (string current3 in IndexItemBase.RemoveDuplicates(this.AccessControlList))
			{
				xElement.Add(new XElement(ns + "Item", current3));
			}
			this.SyndicationItem.ElementExtensions.Add(xElement.CreateReader());
			xElement = new XElement(ns + "VirtualPath");
			foreach (string current4 in this.VirtualPathNodes)
			{
				xElement.Add(new XElement(ns + "Item", current4.Replace(" ", "")));
			}
			this.SyndicationItem.ElementExtensions.Add(xElement.CreateReader());
			var stringBuilder = new StringBuilder();
			using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, new XmlWriterSettings
			{
				CheckCharacters = false
			}))
			{
				this.SyndicationItem.GetAtom10Formatter().WriteTo(xmlWriter);
			}
			return stringBuilder.ToString();
		}

		private static HashSet<string> RemoveDuplicates(IEnumerable<string> inputList)
		{
			return new HashSet<string>(inputList, StringComparer.OrdinalIgnoreCase);
		}

		private static string StripHtml(string input)
		{
			string result;
			using (System.IO.StringWriter stringWriter = new System.IO.StringWriter(System.Globalization.CultureInfo.InvariantCulture))
			{
				foreach (HtmlFragment current in new HtmlStreamReader(input, ParserOptions.TagNamesToUpper))
				{
					if (current.FragmentType == HtmlFragmentType.Text)
					{
						current.ToWriter(stringWriter);
					}
				}
				result = HttpUtility.HtmlDecode(stringWriter.ToString());
			}
			return result;
		}
	}
}
