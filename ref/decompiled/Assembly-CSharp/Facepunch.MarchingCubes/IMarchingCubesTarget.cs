using UnityEngine;

namespace Facepunch.MarchingCubes;

public interface IMarchingCubesTarget
{
	Mesh TargetMesh { get; }

	Mesh TargetMeshForCollision { get; }

	MeshCollider TargetMeshCollider { get; }

	SDFSet SDFSet { get; }

	Vector3 VertexOffset { get; }

	float VertexScale { get; }

	bool WantsConvexCollider { get; }

	bool isClient { get; }

	int LodMeshCount { get; }

	Mesh GetLodMesh(int level);

	void OnRenderMeshesUpdated();
}
