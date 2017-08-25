using System.Runtime.InteropServices;

namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, Size = 16)]
	internal struct PropVariant
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		internal short variantType;
		[System.Runtime.InteropServices.FieldOffset(8)]
		internal System.IntPtr pointerValue;
		[System.Runtime.InteropServices.FieldOffset(8)]
		internal byte byteValue;
		[System.Runtime.InteropServices.FieldOffset(8)]
		internal long longValue;
		internal static PropVariant Empty
		{
			get
			{
				return new PropVariant
				{
					variantType = 0
				};
			}
		}
		internal void FromObject(object obj)
		{
			if (obj.GetType() == typeof(string))
			{
				this.variantType = 31;
				this.pointerValue = System.Runtime.InteropServices.Marshal.StringToHGlobalUni((string)obj);
			}
		}
		public override string ToString()
		{
			if (this.pointerValue != System.IntPtr.Zero)
			{
				return System.Runtime.InteropServices.Marshal.PtrToStringUni(this.pointerValue);
			}
			return string.Empty;
		}
	}
	[System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
	internal struct PROPVARIANT
	{
		internal short vt;
		internal short wReserved1;
		internal short wReserved2;
		internal short wReserved3;
		internal System.IntPtr data;
	}
}
