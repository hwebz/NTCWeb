namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.Guid("0000013A-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
	[System.Runtime.InteropServices.ComImport]
	internal interface IPropertySetStorage
	{
		uint Create([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] [System.Runtime.InteropServices.In] ref System.Guid rfmtid, [System.Runtime.InteropServices.In] System.IntPtr pclsid, [System.Runtime.InteropServices.In] int grfFlags, [System.Runtime.InteropServices.In] int grfMode, ref IPropertyStorage propertyStorage);
		int Open([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] [System.Runtime.InteropServices.In] ref System.Guid rfmtid, [System.Runtime.InteropServices.In] int grfMode, ref IPropertyStorage propertyStorage);
	}
}
