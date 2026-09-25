using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Steam DLC Item")]
public class SteamDLCItem : ScriptableObject
{
	public Translate.Phrase dlcName;

	public int dlcAppID;

	public bool bypassLicenseCheck;

	public bool HasLicense(BasePlayer player)
	{
		if (bypassLicenseCheck)
		{
			return true;
		}
		if (!player.DefaultSkinAccess)
		{
			return player.AllSkinsUnlocked;
		}
		if (!PlatformService.Instance.IsValid)
		{
			return false;
		}
		return PlatformService.Instance.PlayerOwnsDownloadableContent(player.userID, dlcAppID);
	}

	public bool CanUse(BasePlayer player)
	{
		if (!player.DefaultSkinAccess)
		{
			if (!player.AllSkinsUnlocked)
			{
				return bypassLicenseCheck;
			}
			return true;
		}
		if (player.isServer)
		{
			if (!HasLicense(player))
			{
				return (ulong)player.userID < 10000000;
			}
			return true;
		}
		return false;
	}
}
