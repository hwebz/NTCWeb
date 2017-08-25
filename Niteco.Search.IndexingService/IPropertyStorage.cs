namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.ComVisible(true), System.Runtime.InteropServices.Guid("00000138-0000-0000-C000-000000000046"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
	[System.Runtime.InteropServices.ComImport]
	internal interface IPropertyStorage
	{
		int ReadMultiple(uint numProperties, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] ref PropSpec propertySpecification, ref PropVariant propertyValues);
		int WriteMultiple(uint numProperties, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] ref PropSpec propertySpecification, ref PropVariant propertyValues, int propIDNameFirst);
		uint Commit(int commitFlags);
	}
}
