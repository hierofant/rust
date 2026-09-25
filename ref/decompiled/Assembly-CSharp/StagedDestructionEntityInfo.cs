using System;
using System.Collections.Generic;
using UnityEngine;

public class StagedDestructionEntityInfo : PrefabAttribute
{
	[Serializable]
	public class LodVisual
	{
		public Mesh Mesh;

		public string[] MaterialAssetPaths = Array.Empty<string>();

		public void LoadMaterials(List<Material> buffer)
		{
			if (MaterialAssetPaths == null || MaterialAssetPaths.Length == 0)
			{
				return;
			}
			string[] materialAssetPaths = MaterialAssetPaths;
			foreach (string text in materialAssetPaths)
			{
				if (!string.IsNullOrEmpty(text))
				{
					Material material = FileSystem.Load<Material>(text);
					if (material != null)
					{
						buffer.Add(material);
					}
				}
			}
		}
	}

	[Serializable]
	public class StageCollider
	{
		[Tooltip("Collider mesh for this stage. Leave empty to disable this collider while the stage is active.")]
		public Mesh Mesh;

		public string SourceName;
	}

	[Serializable]
	public class ResourceStage
	{
		public float Health;

		[Tooltip("The model this stage's visuals are baked from.")]
		public GameObjectRef VisualSourceAsset;

		[Tooltip("The _col model this stage's collider meshes are baked from.")]
		public GameObjectRef CollisionSourceAsset;

		[Tooltip("One entry per MeshCollider in the entity's Resource Mesh Colliders, in the same order.")]
		public StageCollider[] Colliders = Array.Empty<StageCollider>();

		[Tooltip("One entry per LOD level, in LOD order. Entities driven by a MeshLOD use just the meshes; entities driven by a StagedRendererLOD use the materials too.")]
		public LodVisual[] VisualLods;
	}

	public ResourceStage[] Stages;

	public StageCollider[] GetColliders(int stageIndex)
	{
		stageIndex = Mathf.Clamp(stageIndex, 0, Stages.Length);
		return Stages[stageIndex].Colliders;
	}

	public float GetHealth(int index)
	{
		index = Mathf.Clamp(index, 0, Stages.Length);
		return Stages[index].Health;
	}

	public LodVisual[] GetLodVisuals(int index)
	{
		index = Mathf.Clamp(index, 0, Stages.Length);
		return Stages[index].VisualLods;
	}

	protected override Type GetIndexedType()
	{
		return typeof(StagedDestructionEntityInfo);
	}
}
