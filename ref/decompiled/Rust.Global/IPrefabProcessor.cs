using UnityEngine;

public interface IPrefabProcessor
{
	void RemoveComponent(Component component);

	void NominateForDeletion(GameObject obj);

	void DeleteGameObject(GameObject obj);

	void MarkPropertiesDirty(Object obj);
}
