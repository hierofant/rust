using System.Collections.Generic;
using UnityEngine;

public class GenerateRiverMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0.1f;

	public const bool SnapToTerrain = false;

	public Mesh RiverMesh;

	public Mesh RiverInteriorMesh;

	public Mesh RiverInteriorFrontCapMesh;

	public Mesh RiverInteriorBackCapMesh;

	public Mesh[] RiverMeshes;

	public Material RiverMaterial;

	public PhysicsMaterial RiverPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		RiverMeshes = new Mesh[1] { RiverMesh };
		foreach (PathList river in TerrainMeta.Path.Rivers)
		{
			GameObject gameObject = new GameObject(river.Name);
			List<PathList.MeshObject> list = river.CreateMesh(RiverMeshes, 0.1f, snapToTerrain: false, !river.Path.Circular, !river.Path.Circular, scaleWidthWithLength: true, topAligned: false, 4);
			for (int i = 0; i < list.Count; i++)
			{
				PathList.MeshObject meshObject = list[i];
				GameObject obj = new GameObject("River Mesh");
				obj.transform.position = meshObject.Position;
				obj.tag = "River";
				obj.layer = 4;
				obj.transform.SetParent(gameObject.transform, worldPositionStays: true);
				obj.SetActive(value: false);
				MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RiverPhysicMaterial;
				meshCollider.sharedMesh = meshObject.Meshes[0];
				obj.AddComponent<RiverInfo>();
				WaterBody waterBody = obj.AddComponent<WaterBody>();
				waterBody.Type = WaterBodyType.River;
				waterBody.FishingType = WaterBody.FishingTag.River;
				obj.AddComponent<AddToWaterMap>();
				obj.SetActive(value: true);
			}
		}
	}
}
