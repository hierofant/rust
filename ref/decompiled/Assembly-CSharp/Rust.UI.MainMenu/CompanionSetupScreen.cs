using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class CompanionSetupScreen : UI_Window
{
	public enum ScreenState
	{
		Loading,
		Error,
		NoServer,
		NotSupported,
		NotPushEnabled,
		NotInstalled,
		Disabled,
		Enabled,
		ShowHelp
	}

	public const string PairedKey = "companionPaired";

	public GameObject pleaseSignInMessage;

	public GameObject loadingMessage;

	public GameObject errorMessage;

	public GameObject notSupportedMessage;

	public GameObject notPushEnabledMessage;

	public GameObject disabledMessage;

	public GameObject enabledMessage;

	public GameObject refreshButton;

	public GameObject enableButton;

	public GameObject disableButton;

	public GameObject pairButton;

	public RustText serverName;

	public FlexTransition removeFooterTransition;
}
