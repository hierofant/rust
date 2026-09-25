using UnityEngine;

public class DoorWithLock : Door
{
	[Header("Lock Settings")]
	[ItemSelector]
	public GameObjectRef lockObject;
}
