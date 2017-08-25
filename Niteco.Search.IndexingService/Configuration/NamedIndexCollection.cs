using System.Configuration;

namespace Niteco.Search.IndexingService.Configuration
{
	[ConfigurationCollection(typeof(NamedIndexElement))]
	public class NamedIndexCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}
		public NamedIndexElement this[int index]
		{
			get
			{
				return (NamedIndexElement)base.BaseGet(index);
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
			return new NamedIndexElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((NamedIndexElement)element).Name;
		}
		public void Remove(NamedIndexElement element)
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
