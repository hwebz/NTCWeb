using System;
using System.Configuration;
using EPiServer.Framework.Configuration;

namespace Niteco.Common.Search.Configuration
{
	[ConfigurationCollection(typeof(NamedIndexingServiceElement))]
	public class NamedIndexingServiceCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}
		public NamedIndexingServiceElement this[int index]
		{
			get
			{
				return (NamedIndexingServiceElement)base.BaseGet(index);
			}
			set
			{
				if (base.BaseGet(index) != null)
				{
					base.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}
		public void Add(ConfigurationElement element)
		{
			this.BaseAdd(element);
		}
		public void Clear()
		{
			base.BaseClear();
		}
		protected override ConfigurationElement CreateNewElement()
		{
			return new NamedIndexingServiceElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((NamedIndexingServiceElement)element).Name;
		}
		public void Remove(NamedIndexingServiceElement element)
		{
			base.BaseRemove(element.Name);
		}
		public void Remove(string name)
		{
			base.BaseRemove(name);
		}
		public void RemoveAt(int index)
		{
			base.BaseRemoveAt(index);
		}
	}
}
