using UnityEngine;

public interface IEaselPaintable
{
	GameObject GameObject { get; }

	NetworkableId EaselId { get; set; }

	EaselDeployable parentEasel { get; }

	void AddToEasel(BaseEntity parent);

	void RemoveFromEasel();

	void SaveSignageToItem(Item createdItem);
}
