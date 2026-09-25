using UnityEngine;
using UnityEngine.UI;

public class IconCharm : MonoBehaviour, IClientComponent
{
	public Image Icon;

	public GameObject ClearRoot;

	public Tooltip tooltip;

	public GameObject currentlySelectedRoot;

	public static Translate.Phrase clearPhrase = new Translate.Phrase("charms.clear", "Clear");
}
