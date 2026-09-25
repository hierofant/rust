using System;

namespace Epic.OnlineServices.AntiCheatClient;

internal struct UnprotectMessageOptionsInternal
{
	public int m_ApiVersion;

	public uint m_DataLengthBytes;

	public IntPtr m_Data;

	public uint m_OutBufferSizeBytes;
}
