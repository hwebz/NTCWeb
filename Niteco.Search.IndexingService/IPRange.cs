using System.Net;
using System.Net.Sockets;

namespace Niteco.Search.IndexingService
{
	internal class IPRange
	{
		private AddressFamily _addressFamily;
		private byte[] _addressBytes;
		private byte[] _maskBytes;
		public IPRange(AddressFamily addressFamily, byte[] addressBytes, byte[] maskBytes)
		{
			this._addressFamily = addressFamily;
			this._addressBytes = addressBytes;
			this._maskBytes = maskBytes;
		}
		public bool IsInRange(IPAddress clientAddress)
		{
			if (this._addressFamily != clientAddress.AddressFamily)
			{
				return false;
			}
			byte[] addressBytes = clientAddress.GetAddressBytes();
			for (int i = 0; i < this._addressBytes.Length; i++)
			{
				if ((this._addressBytes[i] & this._maskBytes[i]) != (addressBytes[i] & this._maskBytes[i]))
				{
					return false;
				}
			}
			return true;
		}
		public static bool TryParse(AddressFamily addressFamily, string value, out IPRange result)
		{
			result = null;
			int num;
			if (addressFamily == AddressFamily.InterNetwork)
			{
				num = 32;
			}
			else
			{
				if (addressFamily != AddressFamily.InterNetworkV6)
				{
					return false;
				}
				num = 128;
			}
			if (string.IsNullOrEmpty(value))
			{
				return false;
			}
			System.Collections.BitArray bitArray = new System.Collections.BitArray(num, true);
			int num2 = value.IndexOf('/');
			if (num2 >= 0)
			{
				int num3 = num;
				string text = value.Substring(num2 + 1);
				value = value.Substring(0, num2);
				if (int.TryParse(text, out num3))
				{
					if (num3 >= 0 && num3 <= num)
					{
						for (int i = num3; i < num; i++)
						{
							bitArray.Set(i, false);
						}
					}
				}
				else
				{
					IPAddress iPAddress;
					if (IPAddress.TryParse(text, out iPAddress))
					{
						bitArray = new System.Collections.BitArray(iPAddress.GetAddressBytes());
					}
				}
			}
			IPAddress iPAddress2;
			if (IPAddress.TryParse(value, out iPAddress2))
			{
				byte[] addressBytes = iPAddress2.GetAddressBytes();
				byte[] array = new byte[addressBytes.Length];
				bitArray.CopyTo(array, 0);
				result = new IPRange(addressFamily, addressBytes, array);
				return true;
			}
			return false;
		}
	}
}
