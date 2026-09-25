using UnityEngine;
using UnityEngine.UIElements;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_RefreshButton : RustButton
{
	[SerializeField]
	private UI_LoadingRotate loadingRotate;

	[SerializeField]
	private Image refreshOverview;

	[SerializeField]
	private RustText text;

	private Translate.Phrase _refreshPhrase = new Translate.Phrase("serverbrowser.refresh", "Refresh");

	private Translate.Phrase _cancelPhrase = new Translate.Phrase("serverbrowser.cancel", "Cancel");

	public void SetRefreshState(bool state)
	{
		if (!(loadingRotate == null))
		{
			if (state)
			{
				loadingRotate.ContinuouslyRotate(state: true);
				SetToggleVisualOn();
				text.SetPhrase(_cancelPhrase);
			}
			else
			{
				loadingRotate.Reset();
				loadingRotate.ContinuouslyRotate(state: false);
				SetToggleVisualOff();
				text.SetPhrase(_refreshPhrase);
			}
		}
	}
}
