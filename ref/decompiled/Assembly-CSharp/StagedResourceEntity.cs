using System;
using Facepunch;
using ProtoBuf;
using Rust.Rendering.IndirectInstancing;
using UnityEngine;
using UnityEngine.Serialization;

public class StagedResourceEntity : ResourceEntity
{
	[Header("Staged Resource Entity")]
	public GameObjectRef changeStageEffect;

	[Tooltip("The LOD component whose visuals are swapped per destruction stage. Supports MeshLOD (meshes only) and StagedRendererLOD (meshes and materials).")]
	[FormerlySerializedAs("ResourceMeshLod")]
	public InstancedLODComponent LODComponent;

	public MeshCollider[] ResourceMeshColliders = Array.Empty<MeshCollider>();

	protected int stage;

	private Action UpdateNetworkStageAction;

	private StagedDestructionEntityInfo cachedInfo;

	public int GetStage()
	{
		return stage;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.resource != null)
		{
			int num = info.msg.resource.stage;
			if (info.fromDisk && base.isServer)
			{
				health = startHealth;
				num = 0;
			}
			if (num != stage)
			{
				stage = num;
				UpdateStage();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.resource == null)
		{
			info.msg.resource = Pool.Get<BaseResource>();
		}
		info.msg.resource.health = Health();
		info.msg.resource.stage = stage;
	}

	protected override void OnHealthChanged()
	{
		if (UpdateNetworkStageAction == null)
		{
			UpdateNetworkStageAction = UpdateNetworkStage;
		}
		Invoke(UpdateNetworkStageAction, 0.1f);
	}

	public virtual void UpdateNetworkStage()
	{
		if (FindBestStage() != stage)
		{
			stage = FindBestStage();
			SendNetworkUpdate();
			UpdateStage();
		}
	}

	private int FindBestStage()
	{
		float num = Mathf.InverseLerp(0f, MaxHealth(), Health());
		StagedDestructionEntityInfo.ResourceStage[] stages = GetInfo().Stages;
		for (int i = 0; i < stages.Length; i++)
		{
			if (num >= stages[i].Health)
			{
				return i;
			}
		}
		return stages.Length - 1;
	}

	private StagedDestructionEntityInfo GetInfo()
	{
		if (cachedInfo != null)
		{
			return cachedInfo;
		}
		if (base.isServer)
		{
			cachedInfo = PrefabAttribute.server.Find<StagedDestructionEntityInfo>(prefabID);
		}
		return cachedInfo;
	}

	protected void UpdateStage()
	{
		if (GetInfo().Stages.Length == 0)
		{
			return;
		}
		StagedDestructionEntityInfo.StageCollider[] colliders = cachedInfo.GetColliders(stage);
		int num = ((colliders != null) ? colliders.Length : 0);
		for (int i = 0; i < ResourceMeshColliders.Length; i++)
		{
			MeshCollider meshCollider = ResourceMeshColliders[i];
			if (!(meshCollider == null))
			{
				Mesh mesh = ((i < num) ? colliders[i] : null)?.Mesh;
				if (meshCollider.sharedMesh != mesh)
				{
					meshCollider.sharedMesh = mesh;
				}
				bool flag = mesh != null;
				if (meshCollider.enabled != flag)
				{
					meshCollider.enabled = flag;
				}
			}
		}
		GroundWatch.PhysicsChanged(base.gameObject);
	}
}
