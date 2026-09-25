using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace Rust.UI.MainMenu;

public class UI_TutorialModal : UI_Window
{
	[SerializeField]
	private RustButton _acceptButton;

	[SerializeField]
	private List<RustButton> _declineButtons;

	[SerializeField]
	private VideoPlayer _videoPlayer;
}
