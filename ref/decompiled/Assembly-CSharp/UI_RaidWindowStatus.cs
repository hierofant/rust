using Rust.UI;
using UnityEngine;

public class UI_RaidWindowStatus : MonoBehaviour, IClientComponent
{
	public GameObject openObject;

	public GameObject closedObject;

	public RustText openText;

	public RustText closedText;

	public static readonly Translate.Phrase ClosesIn = new Translate.Phrase("raidwindow.hud.closesin", "Closes in {0}");

	public static readonly Translate.Phrase OpensIn = new Translate.Phrase("raidwindow.hud.opensin", "Opens in {0}");
}
