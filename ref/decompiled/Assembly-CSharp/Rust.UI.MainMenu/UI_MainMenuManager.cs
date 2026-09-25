using System;
using System.Collections.Generic;
using Development.Attributes;
using UnityEngine;

namespace Rust.UI.MainMenu;

[ResetStaticFields]
public class UI_MainMenuManager : SingletonComponent<UI_MainMenuManager>
{
	public const string DisableMenuUIWarmup = "Facepunch.DisableMenuUIWarmup";

	[Header("Tabs")]
	public RectTransform pageParent;

	public GameObjectRef[] pagePrefabs;

	private List<UI_Page> pageInstances = new List<UI_Page>();

	[Space]
	[SerializeField]
	private UI_Popup _genericPopupPrefab;

	[SerializeField]
	private Transform _genericPopupParent;

	[SerializeField]
	[Header("Background Image Settings")]
	private CanvasGroup _homeVideoOverlay;

	[SerializeField]
	private float _homeVideoOverlayAlpha = 1f;

	[SerializeField]
	private float _otherPageVideoOverlayAlpha = 0.98f;

	[SerializeField]
	private CanvasGroup _pageBackgroundOverlay;

	[SerializeField]
	private float _pageBackgroundOverlayAlpha = 0.98f;

	[SerializeField]
	[Space]
	private UI_SafeZoneWarning _safeZoneWarningPopup;

	public static Action OnOpenStateChanged;

	private static bool _isLoaded = false;

	private static bool _isOpen = true;

	public List<GameObject> HideInMenu = new List<GameObject>();

	public List<GameObject> HideInGame = new List<GameObject>();

	public Transform GenericPopupParent => _genericPopupParent;

	public static bool IsLoaded => _isLoaded;

	public static bool IsOpen => _isOpen;
}
