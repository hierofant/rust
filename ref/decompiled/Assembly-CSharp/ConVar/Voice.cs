using UnityEngine;

namespace ConVar;

[Factory("voice")]
public class Voice : ConsoleSystem
{
	[ClientVar(Saved = true, Help = "(Generated) When enabled, routes the local player microphone input back through the voice chat system so they can hear their own voice; useful for testing microphone levels")]
	public static bool loopback = false;

	private static float _voiceRangeBoostAmount = 50f;

	[ReplicatedVar]
	public static float voiceRangeBoostAmount
	{
		get
		{
			return _voiceRangeBoostAmount;
		}
		set
		{
			_voiceRangeBoostAmount = Mathf.Clamp(value, 0f, 200f);
		}
	}

	[ServerVar(Help = "Enabled/disables voice range boost for a player eg. ToggleVoiceRangeBoost sam 1")]
	public static void ToggleVoiceRangeBoost(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		if (player == null)
		{
			arg.ReplyWith("Invalid player: " + arg.GetString(0));
			return;
		}
		bool @bool = arg.GetBool(1);
		player.SetPlayerFlag(BasePlayer.PlayerFlags.VoiceRangeBoost, @bool);
		arg.ReplyWith($"Set {player.displayName} volume boost to {@bool}");
	}
}
