using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1017)]
public class UI_Console : UI_Window
{
	public static UI_Console Instance;

	[Header("Console")]
	public UI_Console_Display consoleDisplay;

	public ConsoleTMPInputField consoleInputField;

	public CanvasGroup consoleGroup;

	public GameObject scrollbar;

	[Header("Extra")]
	public UI_Console_CommandList commandList;

	public Image DarkenImage;

	public float DarkenAlpha = 0.15f;

	public float DarkenSpeed = 8f;

	[Header("Console - Autocomplete")]
	public UI_Autocomplete autocompletePrefab;

	public RectTransform autocompleteParent;

	private UI_Autocomplete autocomplete;
}
