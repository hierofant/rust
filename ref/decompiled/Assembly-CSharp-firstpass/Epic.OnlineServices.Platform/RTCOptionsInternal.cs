using System;

namespace Epic.OnlineServices.Platform;

internal struct RTCOptionsInternal : ISettable<RTCOptions>, IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_PlatformSpecificOptions;

	private RTCBackgroundMode m_BackgroundMode;

	public void Set(ref RTCOptions other)
	{
		Dispose();
		m_ApiVersion = 2;
		m_PlatformSpecificOptions = other.PlatformSpecificOptions;
		m_BackgroundMode = other.BackgroundMode;
	}

	public void Dispose()
	{
		Helper.Dispose(ref m_PlatformSpecificOptions);
	}
}
