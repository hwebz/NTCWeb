using System.Runtime.InteropServices;

namespace Niteco.Search.IndexingService
{
	internal class ole32
	{
		[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Size = 12)]
		internal struct STGOptions
		{
			[System.Runtime.InteropServices.FieldOffset(0)]
			private ushort usVersion;
			[System.Runtime.InteropServices.FieldOffset(2)]
			private ushort reserved;
			[System.Runtime.InteropServices.FieldOffset(4)]
			private uint uiSectorSize;
			[System.Runtime.InteropServices.FieldOffset(8)]
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			private string pwcsTemplateFile;
		}
		private ole32()
		{
		}
		[System.Runtime.InteropServices.DllImport("ole32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint StgCreateStorageEx([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string name, int accessMode, int storageFileFormat, int fileBuffering, System.IntPtr options, System.IntPtr reserved, ref System.Guid riid, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Interface)] ref IPropertySetStorage propertySetStorage);
		[System.Runtime.InteropServices.DllImport("ole32.dll", CharSet = CharSet.Unicode)]
		internal static extern uint StgOpenStorageEx([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string name, int accessMode, int storageFileFormat, int fileBuffering, System.IntPtr options, System.IntPtr reserved, ref System.Guid riid, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Interface)] ref IPropertySetStorage propertySetStorage);
	}
}
