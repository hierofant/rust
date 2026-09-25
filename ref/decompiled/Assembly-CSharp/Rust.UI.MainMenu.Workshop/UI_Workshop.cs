using UnityEngine;

namespace Rust.UI.MainMenu.Workshop;

public class UI_Workshop : UI_Page
{
	public static UI_Workshop Instance;

	[SerializeField]
	private UI_WorkshopItemList itemList;

	[SerializeField]
	private RustButton initialTabButton;

	public static Translate.Phrase loading_workshop = new TokenisedPhrase("loading.workshop", "Loading Workshop");

	public static Translate.Phrase loading_workshop_setup = new TokenisedPhrase("loading.workshop.initializing", "Setting Up Scene");

	public static Translate.Phrase loading_workshop_skinnables = new TokenisedPhrase("loading.workshop.skinnables", "Getting Skinnables");

	public static Translate.Phrase loading_workshop_item = new TokenisedPhrase("loading.workshop.item", "Loading Item Data");

	private readonly Translate.Phrase createNewSkinPhrase = new Translate.Phrase("workshop.createskin.title", "Create skin");

	private readonly Translate.Phrase createNewSkinBodyPhrase = new Translate.Phrase("workshop.createskin.body", "Do you want to create a new skin? This will load the workshop scene.");

	private readonly Translate.Phrase yesPhrase = new Translate.Phrase("workshop.continue", "Continue");

	private readonly Translate.Phrase cancelPhrase = new Translate.Phrase("workshop.cancel", "Cancel");
}
