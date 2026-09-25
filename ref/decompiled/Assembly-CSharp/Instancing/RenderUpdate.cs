using UnityEngine;

namespace Instancing;

public struct RenderUpdate
{
	public uint PrefabId;

	public NetworkableId NetworkId;

	public int Grade;

	public ulong Skin;

	public ulong ModelState;

	public Vector3 Position;

	public Quaternion Rotation;

	public Color CustomColor;

	public int CustomColorIndex;

	public bool IsGlobalUpdate;

	public bool InsideNetworkRange;
}
