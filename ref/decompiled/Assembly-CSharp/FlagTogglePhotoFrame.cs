using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class FlagTogglePhotoFrame : PhotoFrame, IFlagNotify
{
	public GameObjectRef IoEntity;

	public Transform IoEntityAnchor;

	[Tooltip("Flags to toggle on the PhotoFrame entity when child IOEntity's flag changes.")]
	public Flags flagsToToggle;

	private EntityRef<IOEntity> spawnedIo;

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.simpleUID != null)
		{
			spawnedIo.uid = info.msg.simpleUID.uid;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!Rust.Application.isLoadingSave)
		{
			SpawnIOEnt();
		}
	}

	public void OnFlagToggled(bool state)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(flagsToToggle, state);
	}

	private void SpawnIOEnt()
	{
		if (IoEntity.isValid && IoEntityAnchor != null)
		{
			IOEntity iOEntity = GameManager.server.CreateEntity(IoEntity.resourcePath, IoEntityAnchor.position, IoEntityAnchor.rotation) as IOEntity;
			iOEntity.SetParent(this, worldPositionStays: true);
			spawnedIo.Set(iOEntity);
			iOEntity.Spawn();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.simpleUID == null)
		{
			info.msg.simpleUID = Pool.Get<SimpleUID>();
		}
		info.msg.simpleUID.uid = spawnedIo.uid;
	}
}
