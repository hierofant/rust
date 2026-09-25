using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class IconSkinPicker : MonoBehaviour
{
	public GameObjectRef pickerIcon;

	public GameObject container;

	public Action skinChangedEvent;

	public Action<AccessoryItem> charmChangedEvent;

	public ScrollRect scroller;

	public SearchFilterInput searchFilter;

	[Header("Charms")]
	public CharmPicker charmPicker;

	public Image currentCharmIcon;

	public GameObject noCharmIcon;

	[Space]
	public RustButton skinsButton;

	public RustButton charmsButton;

	public static Translate.Phrase defaultSkin = new Translate.Phrase("skin.default", "Default");

	public static Translate.Phrase randomSkin = new Translate.Phrase("skin.random", "Random");
}
