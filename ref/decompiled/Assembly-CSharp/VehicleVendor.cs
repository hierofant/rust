using Facepunch;
using ProtoBuf;

public class VehicleVendor : NPCTalking
{
	public EntityRef spawnerRef;

	public VehicleSpawner vehicleSpawner;

	public VehicleSpawner GetVehicleSpawner()
	{
		if (!spawnerRef.IsValid(base.isServer))
		{
			return null;
		}
		return spawnerRef.Get(base.isServer).GetComponent<VehicleSpawner>();
	}

	public override void UpdateFlags()
	{
		base.UpdateFlags();
		VehicleSpawner vehicleSpawner = GetVehicleSpawner();
		bool b = vehicleSpawner != null && vehicleSpawner.IsPadOccupied();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (spawnerRef.IsValid(serverside: true) && vehicleSpawner == null)
		{
			vehicleSpawner = GetVehicleSpawner();
		}
		else if (vehicleSpawner != null && !spawnerRef.IsValid(serverside: true))
		{
			spawnerRef.Set(vehicleSpawner);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.vehicleVendor = Pool.Get<ProtoBuf.VehicleVendor>();
		info.msg.vehicleVendor.spawnerRef = spawnerRef.uid;
	}

	public override ConversationData GetConversationFor(BasePlayer player)
	{
		return conversations[0];
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.vehicleVendor != null)
		{
			spawnerRef.id_cached = info.msg.vehicleVendor.spawnerRef;
		}
	}
}
