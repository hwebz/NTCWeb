using System.Configuration;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Niteco.Search.IndexingService.Configuration
{
	public class ClientElement : ConfigurationElement
	{
		private object _lockObj = new object();
		private System.Collections.Generic.List<IPAddress> _localIps;
		private System.Collections.Generic.List<IPRange> _ip6Ranges;
		private System.Collections.Generic.List<IPRange> _ip4Ranges;
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
			set
			{
				base["name"] = value;
			}
		}
		[ConfigurationProperty("description", IsRequired = false)]
		public string Description
		{
			get
			{
				return (string)base["description"];
			}
			set
			{
				base["description"] = value;
			}
		}
		[ConfigurationProperty("ipAddress", IsRequired = false)]
		public string IPAddress
		{
			get
			{
				return (string)base["ipAddress"];
			}
			set
			{
				base["ipAddress"] = value;
				this._ip4Ranges = this.ParseIPRangeList(AddressFamily.InterNetwork, value);
			}
		}
		[ConfigurationProperty("ip6Address", IsRequired = false)]
		public string IP6Address
		{
			get
			{
				return (string)base["ip6Address"];
			}
			set
			{
				base["ip6Address"] = value;
				this._ip6Ranges = this.ParseIPRangeList(AddressFamily.InterNetworkV6, value);
			}
		}
		[ConfigurationProperty("allowLocal", IsRequired = false, DefaultValue = false)]
		public bool AllowLocal
		{
			get
			{
				return (bool)base["allowLocal"];
			}
			set
			{
				base["allowLocal"] = value;
			}
		}
		[ConfigurationProperty("readonly", IsRequired = true)]
		public bool ReadOnly
		{
			get
			{
				return (bool)base["readonly"];
			}
			set
			{
				base["readonly"] = value;
			}
		}
		private System.Collections.Generic.List<IPRange> ParseIPRangeList(AddressFamily addressFamily, string list)
		{
			System.Collections.Generic.List<IPRange> list2 = new System.Collections.Generic.List<IPRange>();
			string[] array = list.Split(new char[]
			{
				',',
				' '
			});
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				string text2 = text.Trim();
				IPRange item;
				if (text2.Length > 0 && IPRange.TryParse(addressFamily, text2, out item))
				{
					list2.Add(item);
				}
			}
			return list2;
		}
		private System.Collections.Generic.List<IPAddress> GetLocalAddresses()
		{
			System.Collections.Generic.List<IPAddress> list = new System.Collections.Generic.List<IPAddress>();
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			NetworkInterface[] array = allNetworkInterfaces;
			for (int i = 0; i < array.Length; i++)
			{
				NetworkInterface networkInterface = array[i];
				IPInterfaceProperties iPProperties = networkInterface.GetIPProperties();
				foreach (IPAddressInformation current in iPProperties.UnicastAddresses)
				{
					list.Add(current.Address);
				}
			}
			return list;
		}
		internal bool IsIPAddressAllowed(IPAddress ipAddress)
		{
			if (this.AllowLocal)
			{
				if (this._localIps == null)
				{
					lock (this._lockObj)
					{
						if (this._localIps == null)
						{
							this._localIps = this.GetLocalAddresses();
						}
					}
				}
				if (this._localIps != null)
				{
					foreach (IPAddress current in this._localIps)
					{
						if (current.Equals(ipAddress))
						{
							bool result = true;
							return result;
						}
					}
				}
			}
			if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
			{
				if (this._ip4Ranges == null)
				{
					lock (this._lockObj)
					{
						if (this._ip4Ranges == null)
						{
							this._ip4Ranges = this.ParseIPRangeList(AddressFamily.InterNetwork, this.IPAddress);
						}
					}
				}
				if (this._ip4Ranges == null)
				{
					return false;
				}
				using (System.Collections.Generic.List<IPRange>.Enumerator enumerator2 = this._ip4Ranges.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						IPRange current2 = enumerator2.Current;
						if (current2.IsInRange(ipAddress))
						{
							bool result = true;
							return result;
						}
					}
					return false;
				}
			}
			if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
			{
				if (this._ip6Ranges == null)
				{
					lock (this._lockObj)
					{
						if (this._ip6Ranges == null)
						{
							this._ip6Ranges = this.ParseIPRangeList(AddressFamily.InterNetworkV6, this.IP6Address);
						}
					}
				}
				if (this._ip6Ranges != null)
				{
					foreach (IPRange current3 in this._ip6Ranges)
					{
						if (current3.IsInRange(ipAddress))
						{
							bool result = true;
							return result;
						}
					}
				}
			}
			return false;
		}
	}
}
