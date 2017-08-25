namespace Niteco.Search.IndexingService
{
	[System.Runtime.InteropServices.Guid("89BCB740-6119-101A-BCB7-00DD010655AF"), System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
	[System.Runtime.InteropServices.ComImport]
	internal interface IFilter
	{
		[System.Runtime.InteropServices.PreserveSig]
		IFilterReturnCodes Init([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] IFILTER_INIT grfFlags, int cAttributes, System.IntPtr aAttributes, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] ref IFILTER_FLAGS pdwFlags);
		[System.Runtime.InteropServices.PreserveSig]
		IFilterReturnCodes GetChunk([System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] ref STAT_CHUNK pStat);
		[System.Runtime.InteropServices.PreserveSig]
		IFilterReturnCodes GetText(ref int pcwcBuffer, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] [System.Runtime.InteropServices.Out] System.Text.StringBuilder awcBuffer);
		[System.Runtime.InteropServices.PreserveSig]
		IFilterReturnCodes GetValue(ref System.IntPtr ppPropValue);
		[System.Runtime.InteropServices.PreserveSig]
		IFilterReturnCodes BindRegion(ref FILTERREGION origPos, ref System.Guid riid, ref IUnknown ppunk);
	}
}
