using ConVar;
using UnityEngine;

public class NPCDoorTriggerBox : MonoBehaviour
{
	private Door door;

	private static int playerServerLayer = -1;

	public static SparseGrid<NPCDoorTriggerBox> AllDoors = new SparseGrid<NPCDoorTriggerBox>();

	public void Setup(Door d)
	{
		door = d;
		base.transform.SetParent(door.transform, worldPositionStays: false);
		base.gameObject.layer = 18;
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.isTrigger = true;
		boxCollider.center = Vector3.zero;
		boxCollider.size = Vector3.one * AI.npc_door_trigger_size;
		AllDoors.Add(base.transform.position, this);
	}

	private void OnDestroy()
	{
		AllDoors.Remove(base.transform.position, this);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (door == null || door.isClient || door.IsLocked() || (!door.isSecurityDoor && door.IsOpen()) || (door.isSecurityDoor && !door.IsOpen()))
		{
			return;
		}
		if (playerServerLayer < 0)
		{
			playerServerLayer = LayerMask.NameToLayer("Player (Server)");
		}
		if ((other.gameObject.layer & playerServerLayer) > 0)
		{
			BasePlayer component = other.gameObject.GetComponent<BasePlayer>();
			if (component != null && component.IsNpc && !door.isSecurityDoor)
			{
				door.SetOpen(open: true);
			}
		}
	}

	public bool TryOpenDoorFor(BaseEntity entity)
	{
		if (door == null || door.isClient || door.IsLocked() || door.IsOpen() || door.isSecurityDoor)
		{
			return false;
		}
		if (Vector3.Distance(entity.transform.position, base.transform.position) > AI.npc_door_trigger_size * 2f)
		{
			return false;
		}
		door.SetOpen(open: true);
		return true;
	}
}
