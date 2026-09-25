using UnityEngine;

public class MeshShadowLOD : LODComponent
{
	public float Distance = 100f;

	public float MinDistanceMultiplier = 0.2f;

	public MeshRenderer TargetMesh;
}
