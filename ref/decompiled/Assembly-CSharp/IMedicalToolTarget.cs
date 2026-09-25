public interface IMedicalToolTarget
{
	BaseEntity GetEntity();

	bool IsAlive();

	bool IsValidMedicalToolItem(ItemDefinition itemDef)
	{
		return true;
	}

	void OnMedicalToolApplied(BasePlayer fromPlayer, ItemDefinition itemDef, ItemModConsumable consumable, MedicalTool medicalToolEntity, bool canRevive);
}
