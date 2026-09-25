using Facepunch;
using Facepunch.Extend;
using ProtoBuf;
using UnityEngine;

public class DeepSeaPortal : BaseEntity
{
	public enum PortalModeEnum
	{
		None,
		Entrance,
		Exit
	}

	public MeshRenderer DebugRenderer;

	public GameObjectRef BuoyPrefab;

	public PortalModeEnum PortalMode;

	public CardinalDirection PortalDirection;

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			DeepSeaManager.ServerPortals.Add(this);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			DeepSeaManager.ServerPortals.Remove(this);
			if (PortalMode == PortalModeEnum.Entrance)
			{
				Debug.LogError($"DeepSea: Portal destroyed! Mode={PortalMode}, Direction={PortalDirection}, Position={base.transform.position}\n{StackTraceUtility.ExtractStackTrace()}");
			}
		}
		if (PortalMode == PortalModeEnum.Entrance)
		{
			if (HasFlag(Flags.Open))
			{
				DeepSeaManager.PortalEntranceBounds = default(OBB);
				DeepSeaManager.PortalEntranceTransform = null;
			}
		}
		else if (PortalMode == PortalModeEnum.Exit)
		{
			DeepSeaManager.PortalExitBounds = default(OBB);
			DeepSeaManager.PortalExitTransform = null;
		}
	}

	public void InitBounds()
	{
		if (PortalMode == PortalModeEnum.Entrance)
		{
			if (HasFlag(Flags.Open))
			{
				DeepSeaManager.PortalEntranceBounds = WorldSpaceBounds();
				DeepSeaManager.PortalEntranceTransform = base.transform;
			}
		}
		else if (PortalMode == PortalModeEnum.Exit)
		{
			DeepSeaManager.PortalExitBounds = WorldSpaceBounds();
			DeepSeaManager.PortalExitTransform = base.transform;
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (PortalMode == PortalModeEnum.Entrance)
		{
			bool num = (old & Flags.Open) == Flags.Open;
			bool flag = (next & Flags.Open) == Flags.Open;
			if (num != flag)
			{
				InitBounds();
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InitBounds();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.deepSeaPortal = Pool.Get<ProtoBuf.DeepSeaPortal>();
		Vector3 triggerSize;
		if (BaseNetworkable.UseParallelSaves)
		{
			TransformHandle handle = base.TransformHandle;
			triggerSize = Facepunch.Extend.TransformEx.Unsafe.GetLocalScaleMT(in handle);
		}
		else
		{
			triggerSize = base.transform.localScale;
		}
		info.msg.deepSeaPortal.triggerSize = triggerSize;
		info.msg.deepSeaPortal.portalMode = (int)PortalMode;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.deepSeaPortal != null)
		{
			base.transform.localScale = info.msg.deepSeaPortal.triggerSize;
			PortalMode = (PortalModeEnum)info.msg.deepSeaPortal.portalMode;
			InitBounds();
		}
	}
}
