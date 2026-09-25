using System;
using System.Collections.Generic;
using Rust.UI;
using Rust.UI.MainMenu;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1017)]
public class UI_DeveloperTools : UI_Window
{
	[Serializable]
	private class Tab
	{
		public RustButton button;

		public GameObjectRef windowPrefab;

		public bool adminOnly;

		public bool allowInDemo;

		[NonSerialized]
		public GameObject window;

		[NonSerialized]
		public UI_Window uiWindow;
	}

	public const string DisableDevToolsWarmup = "Facepunch.DisableDevToolsWarmup";

	public static UI_DeveloperTools Instance;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private NeedsCursor needsCursor;

	[SerializeField]
	private NeedsKeyboard needsKeyboard;

	[Space]
	[SerializeField]
	private List<Tab> tabs;

	[SerializeField]
	private RectTransform tabContentParent;

	[SerializeField]
	[Space]
	private Image blurImage;

	[SerializeField]
	private float blurSize;

	[SerializeField]
	private float blurSpread;

	[SerializeField]
	private float consoleBlurSize;

	[SerializeField]
	private float consoleBlurSpread;

	public static bool isOpen
	{
		get
		{
			if (Instance != null)
			{
				return Instance.IsOpen();
			}
			return false;
		}
	}
}
