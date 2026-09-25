using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_SettingsTabButton : MonoBehaviour
{
	public RustButton button;

	public CanvasGroup canvasGroup;

	public GameObject searchResult;

	public RustText searchResultCountText;

	[UnityEvent]
	public void SetSearchResult(int matchCount)
	{
		canvasGroup.alpha = ((matchCount != 0) ? 1f : 0.3f);
		searchResult.SetActive(matchCount != 0);
		if (matchCount != 0)
		{
			searchResultCountText.SetText(matchCount.ToString());
		}
	}

	[UnityEvent]
	public void CancelSearch()
	{
		canvasGroup.alpha = 1f;
		searchResult.SetActive(value: false);
	}
}
