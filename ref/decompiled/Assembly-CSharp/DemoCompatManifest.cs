using System;
using UnityEngine;

public class DemoCompatManifest : ScriptableObject
{
	[Serializable]
	public struct DeprecatedEntry
	{
		[HideInInspector]
		public string str;

		public uint hash;

		public long deprecatedAt;
	}

	[Serializable]
	public struct RemapEntry
	{
		public uint oldHash;

		public string newPath;

		public string guid;

		public long remappedAt;
	}

	public DeprecatedEntry[] entries = Array.Empty<DeprecatedEntry>();

	public RemapEntry[] remaps = Array.Empty<RemapEntry>();

	public static DemoCompatManifest Load()
	{
		return FileSystem.Load<DemoCompatManifest>("Assets/demo_compat_manifest.asset", complain: false);
	}
}
