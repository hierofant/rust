using Rust.UI;
using UnityEngine;

public class UI_ServerAdminUGCEntryAudio : UI_ServerAdminUGCEntry
{
	[SerializeField]
	private AudioSource audioSpeaker;

	[SerializeField]
	[Space]
	private RustText playbackText;

	[SerializeField]
	private RustSlider progressSlider;

	[SerializeField]
	private RustIcon playIcon;
}
