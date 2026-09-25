using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_Popup_CrosshairImportExport : UI_Popup
{
	[SerializeField]
	private bool isExport;

	[SerializeField]
	private RustInput inputField;

	[SerializeField]
	private RustText placeHolderTextField;

	[SerializeField]
	private RustText copyButtonTextField;

	[SerializeField]
	private Image inputFieldBackgroundImage;

	[SerializeField]
	private Color defaultBackgroundColor;

	[SerializeField]
	private Color failedBackgroundColor;

	public Translate.Phrase placeHolderPhrase;

	public Translate.Phrase invalidCodePhrase;

	public Translate.Phrase copyCodePhrase;

	public Translate.Phrase copiedPhrase;
}
