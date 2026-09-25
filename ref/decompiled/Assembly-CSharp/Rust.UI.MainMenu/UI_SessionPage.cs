using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SessionPage : UI_Page
{
	public static UI_SessionPage Instance;

	public HttpImage serverLogoImage;

	[SerializeField]
	private RustText _topServerName;

	[SerializeField]
	private GameObject _rustPlusBanner;

	[SerializeField]
	private GameObject _connectToServerButton;

	[SerializeField]
	private GameObject _quitButton;

	[SerializeField]
	private GameObject _quitDemo;

	[SerializeField]
	private GameObject _quitTutorial;

	[SerializeField]
	private RustButton _rustPlusButton;

	[SerializeField]
	private RustButton _tutorialButton;

	[SerializeField]
	private UI_ConnectModal _connectModal;

	[SerializeField]
	private CompanionSetupScreen _setupScreen;
}
