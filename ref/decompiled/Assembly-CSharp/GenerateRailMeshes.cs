using UnityEngine;

public class GenerateRailMeshes : ProceduralComponent
{
	public const float NormalSmoothing = 0f;

	public const bool SnapToTerrain = false;

	public Mesh RailMesh;

	public Mesh[] RailMeshes;

	public Material RailMaterial;

	public PhysicsMaterial RailPhysicMaterial;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		if (RailMeshes == null || RailMeshes.Length == 0)
		{
			RailMeshes = new Mesh[1] { RailMesh };
		}
		foreach (PathList rail in TerrainMeta.Path.Rails)
		{
			GameObject gameObject = new GameObject(rail.Name);
			foreach (PathMeshTemplate item in rail.CreateMeshTemplates(RailMeshes, 0f, snapToTerrain: false, !rail.Path.Circular && !rail.Start, !rail.Path.Circular && !rail.End))
			{
				GameObject obj = new GameObject("Rail Mesh");
				obj.transform.position = item.Position;
				obj.tag = "Railway";
				obj.SetCustomTag(GameObjectTag.AllowBarricadePlacement, apply: true);
				obj.layer = 16;
				obj.transform.SetParent(gameObject.transform, worldPositionStays: true);
				obj.SetActive(value: false);
				MeshCollider meshCollider = obj.AddComponent<MeshCollider>();
				meshCollider.sharedMaterial = RailPhysicMaterial;
				item.Generate();
				meshCollider.sharedMesh = item.outputMeshes[0];
				obj.AddComponent<AddToHeightMap>();
				obj.SetActive(value: true);
			}
			AddTrackSpline(gameObject, rail);
		}
	}

	private void AddTrackSpline(GameObject rootGO, PathList rail)
	{
		TrainTrackSpline trainTrackSpline = rootGO.AddComponent<TrainTrackSpline>();
		trainTrackSpline.aboveGroundSpawn = rail.Hierarchy == 2;
		trainTrackSpline.hierarchy = rail.Hierarchy;
		if (trainTrackSpline.aboveGroundSpawn)
		{
			TrainTrackSpline.SidingSplines.Add(trainTrackSpline);
		}
		Vector3[] array = new Vector3[rail.Path.Points.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = rail.Path.Points[i];
			array[i].y += 0.41f;
		}
		Vector3[] array2 = new Vector3[rail.Path.Tangents.Length];
		for (int j = 0; j < array.Length; j++)
		{
			array2[j] = rail.Path.Tangents[j];
		}
		trainTrackSpline.SetAll(array, array2, 0.25f);
	}
}
