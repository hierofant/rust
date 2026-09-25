using Rust.UI;
using UnityEngine;

public class UI_ServerAdminServerInfo : MonoBehaviour
{
	[SerializeField]
	private RustText InfoName;

	[SerializeField]
	private RustText InfoValue;

	private static Translate.Phrase HostNamePhrase = new Translate.Phrase("serverinfo.HostName", "Host Name");

	private static Translate.Phrase MaxPlayersPhrase = new Translate.Phrase("serverinfo.MaxPlayers", "Max Players");

	private static Translate.Phrase PlayersPhrase = new Translate.Phrase("serverinfo.Players", "Players");

	private static Translate.Phrase QueuedPhrase = new Translate.Phrase("serverinfo.Queued", "Queued");

	private static Translate.Phrase JoiningPhrase = new Translate.Phrase("serverinfo.Joining", "Joining");

	private static Translate.Phrase ReservedSlotsPhrase = new Translate.Phrase("serverinfo.ReservedSlots", "Reserved Slots");

	private static Translate.Phrase EntityCountPhrase = new Translate.Phrase("serverinfo.EntityCount", "Entity Count");

	private static Translate.Phrase GameTimePhrase = new Translate.Phrase("serverinfo.GameTime", "Game Time");

	private static Translate.Phrase UptimePhrase = new Translate.Phrase("serverinfo.Uptime", "Uptime");

	private static Translate.Phrase MapPhrase = new Translate.Phrase("serverinfo.Map", "Map");

	private static Translate.Phrase FrameratePhrase = new Translate.Phrase("serverinfo.Framerate", "Framerate");

	private static Translate.Phrase MemoryPhrase = new Translate.Phrase("serverinfo.Memory", "Memory");

	private static Translate.Phrase MemoryUsageSystemPhrase = new Translate.Phrase("serverinfo.MemoryUsageSystem", "System Memory Usage");

	private static Translate.Phrase CollectionsPhrase = new Translate.Phrase("serverinfo.Collections", "Garbage Collections");

	private static Translate.Phrase NetworkInPhrase = new Translate.Phrase("serverinfo.NetworkIn", "Network In");

	private static Translate.Phrase NetworkOutPhrase = new Translate.Phrase("serverinfo.NetworkOut", "Network Out");

	private static Translate.Phrase RestartingPhrase = new Translate.Phrase("serverinfo.Restarting", "Restarting");

	private static Translate.Phrase SaveCreatedTimePhrase = new Translate.Phrase("serverinfo.SaveCreatedTime", "Save Created Time");

	private static Translate.Phrase VersionPhrase = new Translate.Phrase("serverinfo.Version", "Version");

	private static Translate.Phrase ProtocolPhrase = new Translate.Phrase("serverinfo.Protocol", "Protocol");
}
