using System;

namespace Epic.OnlineServices.Platform;

internal struct WindowsRTCOptionsInternal : ISettable<WindowsRTCOptions>, IDisposable
{
	private int m_ApiVersion;

	private IntPtr m_PlatformSpecificOptions;

	private RTCBackgroundMode m_BackgroundMode;

	public void Set(ref WindowsRTCOptions other)
	{
		Dispose();
		m_ApiVersion = 2;
		Helper.Set<WindowsRTCOptionsPlatformSpecificOptions, WindowsRTCOptionsPlatformSpecificOptionsInternal>(other.PlatformSpecificOptions, ref m_PlatformSpecificOptions);
		m_BackgroundMode = other.BackgroundMode;
	}

	public void Dispose()
	{
		Helper.Dispose(ref m_PlatformSpecificOptions);
	}
}
