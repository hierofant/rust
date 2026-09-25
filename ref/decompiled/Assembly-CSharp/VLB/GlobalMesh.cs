using UnityEngine;

namespace VLB;

public static class GlobalMesh
{
	private static Mesh ms_Mesh;

	private static Bounds ms_MeshBounds;

	public static Mesh mesh
	{
		get
		{
			if (ms_Mesh == null)
			{
				ms_Mesh = MeshGenerator.GenerateConeZ_Radius(1f, 1f, 1f, Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, cap: true);
				ms_Mesh.hideFlags = Consts.ProceduralObjectsHideFlags;
				ms_MeshBounds = ms_Mesh.bounds;
			}
			return ms_Mesh;
		}
	}

	public static Bounds MeshBounds => ms_MeshBounds;

	public static void Destroy()
	{
		if (ms_Mesh != null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy(ms_Mesh);
			}
			else
			{
				Object.DestroyImmediate(ms_Mesh);
			}
			ms_Mesh = null;
		}
	}
}
