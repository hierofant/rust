using Rust.UI;
using UnityEngine;

public class SortingSettings : MonoBehaviour, IClientComponent
{
	public LootPanel Panel;

	public RustToggle EnabledToggle;

	public RustText LanguageLabel;

	public RustText ModeLabel;

	public RustToggle ReverseToggle;

	public RustToggle StackToggle;

	public GameObject CustomRoot;

	public GameObjectRef SortingIcon;

	public Transform SortingParent;

	public GameObject LanguageRoot;

	public Option[] SortOptions;

	public GameObject BackgroundBlocker;
}
