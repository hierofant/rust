using UnityEngine;

public class SteamClientWrapper : SingletonComponent<SteamClientWrapper>
{
	public Texture2D DefaultAvatar;

	public const ulong MinSteamId = 76500000000000000uL;

	private static readonly Translate.Phrase TimelineDeathTitle = new Translate.Phrase("timeline.death", "Death");

	private static readonly Translate.Phrase TimelineKillTitle = new Translate.Phrase("timeline.kill", "Kill");
}
