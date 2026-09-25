using System;

public class SimpleBuildingBlockModelVariant : PrefabAttribute
{
	public GameObjectRef prefab;

	public BaseEntity.Flags Flag;

	protected override Type GetIndexedType()
	{
		return typeof(SimpleBuildingBlockModelVariant);
	}
}
