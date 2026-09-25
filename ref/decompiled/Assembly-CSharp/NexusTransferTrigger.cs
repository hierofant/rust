using System.Collections.Generic;
using UnityEngine;

public class NexusTransferTrigger : BaseMonoBehaviour, IServerComponent
{
	[Tooltip("Must implement INexusTransferTriggerController!")]
	public MonoBehaviour Controller;

	private static readonly HashSet<BaseEntity> PendingEntities = new HashSet<BaseEntity>();

	private INexusTransferTriggerController _controller;

	protected void Start()
	{
		_controller = Controller as INexusTransferTriggerController;
		if (_controller == null)
		{
			Debug.LogError("NexusTransferTrigger doesn't have a valid controller assigned!", this);
		}
	}

	protected void OnTriggerEnter(Collider other)
	{
		if (_controller == null)
		{
			return;
		}
		var (zoneKey, method) = _controller.GetTransferDestination();
		if (string.IsNullOrEmpty(zoneKey))
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(other.gameObject);
		BaseEntity rootEntity;
		if (!(baseEntity == null))
		{
			rootEntity = NexusServer.FindRootEntity(baseEntity, includeFerry: true);
			if (_controller.CanTransfer(rootEntity) && PendingEntities.Add(rootEntity))
			{
				TransferAndWait();
			}
		}
		async void TransferAndWait()
		{
			try
			{
				await NexusServer.TransferEntity(rootEntity, zoneKey, method);
			}
			finally
			{
				PendingEntities.Remove(rootEntity);
			}
		}
	}
}
