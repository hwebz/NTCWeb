namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.Guid("00000000-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
	[System.Runtime.InteropServices.ComImport]
	internal interface IUnknown
	{
		[System.Runtime.InteropServices.PreserveSig]
		System.IntPtr QueryInterface(ref System.Guid riid, out System.IntPtr pVoid);
		[System.Runtime.InteropServices.PreserveSig]
		System.IntPtr AddRef();
		[System.Runtime.InteropServices.PreserveSig]
		System.IntPtr Release();
	}
}
