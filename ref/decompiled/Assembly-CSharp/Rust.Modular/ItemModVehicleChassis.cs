using UnityEngine;

namespace Rust.Modular;

public class ItemModVehicleChassis : ItemMod, VehicleModuleInformationPanel.IVehicleModuleInfo
{
	public GameObjectRef entityPrefab;

	[Range(1f, 6f)]
	public int socketsTaken = 1;

	public static readonly Translate.Phrase CantMoveChassisError = new Translate.Phrase("error.chassismove", "Cannot move item: Can't move vehicle chassis!");

	public int SocketsTaken => socketsTaken;
}
