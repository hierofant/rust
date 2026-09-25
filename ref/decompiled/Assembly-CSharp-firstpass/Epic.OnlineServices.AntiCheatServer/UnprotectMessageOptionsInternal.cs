using System;

namespace Epic.OnlineServices.AntiCheatServer;

internal struct UnprotectMessageOptionsInternal
{
	public int m_ApiVersion;

	public IntPtr m_ClientHandle;

	public uint m_DataLengthBytes;

	public IntPtr m_Data;

	public uint m_OutBufferSizeBytes;
}
