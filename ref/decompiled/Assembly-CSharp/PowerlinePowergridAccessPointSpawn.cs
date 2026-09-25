using UnityEngine;

public class PowerlinePowergridAccessPointSpawn : MonoBehaviour
{
	private void Awake()
	{
		PowergridManager.Server_AddPowerlineAccessPointSpawn(this);
	}

	private void OnDestroy()
	{
		PowergridManager.Server_RemovePowerlineAccessPointSpawn(this);
	}
}
