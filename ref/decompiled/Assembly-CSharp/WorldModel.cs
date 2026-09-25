using UnityEngine;

public class WorldModel : MonoBehaviour
{
	public float mass = 1f;

	public int GetTriCount(int lod = 0)
	{
		LODGroup componentInChildren = GetComponentInChildren<LODGroup>(includeInactive: true);
		if (componentInChildren == null)
		{
			return 0;
		}
		LOD[] lODs = componentInChildren.GetLODs();
		if (lODs.Length == 0)
		{
			return 0;
		}
		int num = 0;
		Renderer[] renderers = lODs[lod].renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer == null)
			{
				continue;
			}
			if (renderer is MeshRenderer meshRenderer)
			{
				MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
				if (component != null && component.sharedMesh != null)
				{
					num += GetMeshTriangleCount(component.sharedMesh);
				}
			}
			else if (renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh != null)
			{
				num += GetMeshTriangleCount(skinnedMeshRenderer.sharedMesh);
			}
		}
		return num;
	}

	private static int GetMeshTriangleCount(Mesh mesh)
	{
		int num = 0;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			num += (int)mesh.GetIndexCount(i) / 3;
		}
		return num;
	}
}
