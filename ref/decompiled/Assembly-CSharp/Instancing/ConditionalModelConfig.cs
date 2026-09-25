using UnityEngine;

namespace Instancing;

public class ConditionalModelConfig
{
	public ulong ModelStateMask;

	public uint TargetPrefabId;

	public Matrix4x4 LocalToWorld;
}
