using UnityEngine;

namespace ConVar;

[Factory("audio")]
public class Audio : ConsoleSystem
{
	[ClientVar(Help = "Volume", Saved = true)]
	public static float master = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float musicvolume = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float musicvolumemenu = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float game = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float ui = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float voices = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float instruments = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float voiceProps = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float eventAudio = 1f;

	[ClientVar(Help = "Volume", Saved = true)]
	public static float videoPlayer = 1f;

	[ClientVar(Help = "Ambience System")]
	public static bool ambience = true;

	[ClientVar(Help = "Max ms per frame to spend updating sounds")]
	public static float framebudget = 0.3f;

	[ClientVar(Help = "(Generated) Minimum fraction of the audio source pool updated per frame; higher values keep more audio sources in sync at the cost of CPU time")]
	public static float minupdatefraction = 0.1f;

	[ClientVar(Help = "Use more advanced sound occlusion", Saved = true)]
	public static bool advancedocclusion = false;

	[ClientVar(Help = "Use higher quality sound fades on some sounds")]
	public static bool hqsoundfade = false;

	[ClientVar(Saved = false, Help = "(Generated) When enabled, logs voice chat limiting decisions to the console, showing when players are muted due to proximity or bandwidth constraints")]
	public static bool debugVoiceLimiting = false;

	[ClientVar(Help = "(Generated) Enables audio source pooling, reusing AudioSource components instead of creating and destroying them each time a sound plays")]
	public static bool enableSoundPooling = false;

	[ClientVar(Help = "Volume", Saved = true)]
	public static int speakers
	{
		get
		{
			return (int)UnityEngine.AudioSettings.speakerMode;
		}
		set
		{
			value = Mathf.Clamp(value, 2, 7);
		}
	}

	[ClientVar(Help = "(Generated) Prints a list of all currently active audio sources with their asset names, volume, and world position")]
	public static void printSounds(Arg arg)
	{
	}

	[ClientVar(ClientAdmin = true, Help = "print active engine sound info")]
	public static void printEngineSounds(Arg arg)
	{
	}
}
