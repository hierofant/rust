using UnityEngine;

public class ImpostorInstanceData
{
	public ImpostorBatch Batch;

	public int BatchIndex;

	private int hash;

	private Vector4 positionAndScale = Vector4.zero;

	public Renderer Renderer { get; private set; }

	public Mesh Mesh { get; private set; }

	public Material Material { get; private set; }

	public bool DeepSea { get; private set; }

	public ImpostorInstanceData(Renderer renderer, Mesh mesh, Material material)
	{
		Renderer = renderer;
		Mesh = mesh;
		Material = material;
		hash = GenerateHashCode();
		Update();
	}

	public ImpostorInstanceData(Vector3 position, Vector3 scale, Mesh mesh, Material material)
	{
		positionAndScale = new Vector4(position.x, position.y, position.z, scale.x);
		Mesh = mesh;
		Material = material;
		hash = GenerateHashCode();
		Update();
	}

	private int GenerateHashCode()
	{
		return ((17 * 31 + Material.GetHashCode()) * 31 + Mesh.GetHashCode()) * 31 + DeepSea.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		ImpostorInstanceData impostorInstanceData = obj as ImpostorInstanceData;
		if (impostorInstanceData.Material == Material && impostorInstanceData.Mesh == Mesh)
		{
			return impostorInstanceData.DeepSea == DeepSea;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return hash;
	}

	public Vector4 PositionAndScale()
	{
		if (Renderer != null)
		{
			Transform transform = Renderer.transform;
			Vector3 position = transform.position;
			Vector3 lossyScale = transform.lossyScale;
			float w = (Renderer.enabled ? lossyScale.x : (0f - lossyScale.x));
			positionAndScale = new Vector4(position.x, position.y, position.z, w);
		}
		return positionAndScale;
	}

	public void Update()
	{
		Vector4 value = PositionAndScale();
		if (Batch != null)
		{
			Batch.Positions[BatchIndex] = value;
			Batch.IsDirty = true;
		}
		Vector3 position = new Vector3(value.x, value.y, value.z);
		DeepSea = DeepSeaManager.IsInsideDeepSea(position);
	}
}
