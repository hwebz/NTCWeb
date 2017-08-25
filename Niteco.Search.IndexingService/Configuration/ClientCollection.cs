using System.Configuration;

namespace Niteco.Search.IndexingService.Configuration
{
	[ConfigurationCollection(typeof(ClientElement))]
	public class ClientCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}
		public ClientElement this[int index]
		{
			get
			{
				return (ClientElement)base.BaseGet(index);
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
			return new ClientElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ClientElement)element).Name;
		}
		public void Remove(ClientElement element)
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
