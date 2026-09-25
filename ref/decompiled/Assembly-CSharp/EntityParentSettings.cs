using UnityEngine;

public class EntityParentSettings : MonoBehaviour
{
	public bool DetatchChildrenAfterSpawn;

	public bool SkipDisableCollisionInEditor;

	public bool TriggerPostMapEntitySpawn;

	public void TryDetachChildren(BaseEntity entity)
	{
		if (!DetatchChildrenAfterSpawn)
		{
			return;
		}
		BaseEntity[] array = entity.children.ToArray();
		foreach (BaseEntity baseEntity in array)
		{
			baseEntity.SetParent(null, worldPositionStays: true);
			if (TriggerPostMapEntitySpawn)
			{
				baseEntity.PostMapEntitySpawn();
			}
		}
	}
}
