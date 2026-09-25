using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_ServerAdminUGCEntryImage : UI_ServerAdminUGCEntry
{
	[SerializeField]
	private RawImage rawImage;

	[SerializeField]
	private Vector2 originalImageSize;

	[SerializeField]
	[Space]
	private GameObject multiImageRoot;

	[SerializeField]
	private RustText imageIndexText;
}
