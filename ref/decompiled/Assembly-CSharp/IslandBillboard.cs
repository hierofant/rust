using UnityEngine;

public class IslandBillboard : BaseEntity
{
	public Transform BillboardRoot;

	public Transform Billboard;

	public Renderer BillboardRenderer;

	public Material[] randomMats;

	public bool randomScale;

	public float minMaxScale = 0.25f;

	public bool mainIsland;

	public override void ServerInit()
	{
		base.ServerInit();
		DeepSeaManager.ServerBillboards.Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DeepSeaManager.ServerBillboards.Remove(this);
	}
}
