using System.Collections.Generic;
using System.IO;
using Niteco.Search.IndexingService.Configuration;

namespace Niteco.Search.IndexingService
{
	internal class NamedIndex
	{
		private string _namedIndex;
		private Dictionary<string, bool> _fieldInResponse = new Dictionary<string, bool>();
		internal DirectoryInfo DirectoryInfo
		{
			get;
			private set;
		}
		internal DirectoryInfo ReferenceDirectoryInfo
		{
			get;
			private set;
		}
		internal bool IsValid
		{
			get;
			private set;
		}
		internal Lucene.Net.Store.Directory Directory
		{
			get;
			private set;
		}
		internal Lucene.Net.Store.Directory ReferenceDirectory
		{
			get;
			private set;
		}
		internal bool ReadOnly
		{
			get;
			set;
		}
		internal string Name
		{
			get
			{
				return this._namedIndex;
			}
		}
		internal string ReferenceName
		{
			get
			{
				return this._namedIndex + "_ref";
			}
		}
		internal int PendingDeletesOptimizeThreshold
		{
			get;
			set;
		}
		internal NamedIndex() : this(null)
		{
		}
		internal NamedIndex(string name) : this(name, false)
		{
		}
		internal NamedIndex(string name, bool useRefIndex)
		{
			this._namedIndex = (string.IsNullOrEmpty(name) ? IndexingServiceSettings.DefaultIndexName : name);
			if (IndexingServiceSettings.NamedIndexDirectories.ContainsKey(this._namedIndex))
			{
				if (useRefIndex)
				{
					this.Directory = IndexingServiceSettings.ReferenceIndexDirectories[this._namedIndex];
					this.ReferenceDirectoryInfo = IndexingServiceSettings.ReferenceDirectoryInfos[this._namedIndex];
				}
				else
				{
					this.Directory = IndexingServiceSettings.NamedIndexDirectories[this._namedIndex];
					this.ReferenceDirectory = IndexingServiceSettings.ReferenceIndexDirectories[this._namedIndex];
					this.ReferenceDirectoryInfo = IndexingServiceSettings.ReferenceDirectoryInfos[this._namedIndex];
					this.DirectoryInfo = IndexingServiceSettings.MainDirectoryInfos[this._namedIndex];
				}
				NamedIndexElement namedIndexElement = IndexingServiceSettings.NamedIndexElements[this._namedIndex];
				this.PendingDeletesOptimizeThreshold = namedIndexElement.PendingDeletesOptimizeThreshold;
				this.ReadOnly = namedIndexElement.ReadOnly;
				this.SetFieldInResponse(namedIndexElement);
				this.IsValid = true;
			}
		}
		internal bool IncludeInResponse(string defaultFieldName)
		{
			return this._fieldInResponse.ContainsKey(defaultFieldName) && this._fieldInResponse[defaultFieldName];
		}
		private void SetFieldInResponse(NamedIndexElement element)
		{
            this._fieldInResponse.Add(IndexingServiceSettings.DefaultFieldName, false);
            this._fieldInResponse.Add(IndexingServiceSettings.IdFieldName, element.IdFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.TitleFieldName, element.TitleFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.DisplayTextFieldName, element.DisplayTextFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.AuthorsFieldName, element.AuthorFieldInResponse);
            this._fieldInResponse.Add(IndexingServiceSettings.CreatedFieldName, element.CreatedFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.ModifiedFieldName, element.ModifiedFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.UriFieldName, element.UriFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.TypeFieldName, element.TypeFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.CultureFieldName, element.CultureFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.CategoriesFieldName, element.CategoriesFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.AclFieldName, element.AclFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.ReferenceIdFieldName, element.ReferenceFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.MetadataFieldName, element.MetadataFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.VirtualPathFieldName, element.VirtualPathFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.AuthorStorageFieldName, false);
			this._fieldInResponse.Add(IndexingServiceSettings.PublicationEndFieldName, element.PublicationEndFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.PublicationStartFieldName, element.PublicationStartFieldInResponse);
			this._fieldInResponse.Add(IndexingServiceSettings.ItemStatusFieldName, element.ItemStatusFieldInResponse);
		}
	}
}
