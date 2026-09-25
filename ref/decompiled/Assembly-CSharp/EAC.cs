using System;

public static class EAC
{
	[Flags]
	public enum SystemConfig
	{
		None = 0,
		CryptoProcessor = 1,
		SecureBoot = 2,
		KernelCodeIntegrity = 4,
		IOMMU = 8,
		Default = 3
	}
}
