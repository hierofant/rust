public class GameModeSpawnGroup : SpawnGroup
{
	public string[] gameModeTags;

	protected override bool AllowOverlappingSpawns => true;

	protected override bool BlockSpawnedEntitySaving => false;

	public void ResetSpawnGroup()
	{
		Clear();
		SpawnInitial();
	}

	public bool ShouldSpawn()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (activeGameMode == null && HasTag("vanilla"))
		{
			return true;
		}
		if (activeGameMode == null)
		{
			return false;
		}
		if (gameModeTags.Length == 0)
		{
			return true;
		}
		if (activeGameMode.HasAnyGameModeTag(gameModeTags))
		{
			return true;
		}
		return false;
	}

	private bool HasTag(string tag)
	{
		string[] array = gameModeTags;
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Equals(array[i], tag))
			{
				return true;
			}
		}
		return false;
	}

	protected override void Spawn(int numToSpawn)
	{
		if (ShouldSpawn())
		{
			base.Spawn(numToSpawn);
		}
	}
}
