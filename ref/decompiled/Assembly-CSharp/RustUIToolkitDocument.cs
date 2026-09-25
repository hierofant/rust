using System;
using Rust.UI.Toolkit;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class RustUIToolkitDocument : IDisposable
{
	public GameObject GameObject { get; private set; }

	public UIDocument Document { get; private set; }

	public NeedsCursor Cursor { get; private set; }

	public PanelSettings PanelSettings { get; private set; }

	public VisualElement Root
	{
		get
		{
			if (!(Document != null))
			{
				return null;
			}
			return Document.rootVisualElement;
		}
	}

	private RustUIToolkitDocument()
	{
	}

	public static RustUIToolkitDocument Create(Transform parent, string name, float sortingOrder, bool withCursor = false)
	{
		RustUIToolkitDocument rustUIToolkitDocument = new RustUIToolkitDocument
		{
			GameObject = new GameObject(name)
		};
		rustUIToolkitDocument.GameObject.transform.SetParent(parent, worldPositionStays: false);
		if (withCursor)
		{
			rustUIToolkitDocument.Cursor = rustUIToolkitDocument.GameObject.AddComponent<NeedsCursor>();
		}
		rustUIToolkitDocument.PanelSettings = CreatePanelSettings(name, sortingOrder);
		rustUIToolkitDocument.Document = rustUIToolkitDocument.GameObject.AddComponent<UIDocument>();
		rustUIToolkitDocument.Document.panelSettings = rustUIToolkitDocument.PanelSettings;
		rustUIToolkitDocument.Document.sortingOrder = sortingOrder;
		VisualElement rootVisualElement = rustUIToolkitDocument.Document.rootVisualElement;
		if (rootVisualElement != null)
		{
			RustUI.Attach(rootVisualElement);
			rootVisualElement.pickingMode = PickingMode.Ignore;
		}
		return rustUIToolkitDocument;
	}

	public void SetContent(VisualElement content)
	{
		VisualElement root = Root;
		if (root != null && content != null)
		{
			RustUI.Attach(root);
			root.pickingMode = PickingMode.Ignore;
			if (content.parent != root)
			{
				root.Clear();
				root.Add(content);
			}
		}
	}

	public void SetVisible(bool visible)
	{
		if (!(Document == null))
		{
			Document.enabled = true;
			VisualElement rootVisualElement = Document.rootVisualElement;
			if (rootVisualElement != null)
			{
				RustUI.SetVisible(rootVisualElement, visible);
			}
		}
	}

	public void SetCursorEnabled(bool enabled)
	{
		if (Cursor != null)
		{
			Cursor.enabled = enabled;
		}
	}

	public void Dispose()
	{
		if (Document != null && Document.rootVisualElement != null)
		{
			Document.rootVisualElement.Clear();
		}
		if (GameObject != null)
		{
			UnityEngine.Object.Destroy(GameObject);
			GameObject = null;
			Document = null;
			Cursor = null;
		}
		if (PanelSettings != null)
		{
			UnityEngine.Object.Destroy(PanelSettings);
			PanelSettings = null;
		}
	}

	private static PanelSettings CreatePanelSettings(string name, float sortingOrder)
	{
		PanelSettings panelSettings = RustUIHost.PanelSettings;
		PanelSettings panelSettings2 = ScriptableObject.CreateInstance<PanelSettings>();
		panelSettings2.name = name + " PanelSettings";
		panelSettings2.themeStyleSheet = panelSettings.themeStyleSheet;
		panelSettings2.scaleMode = panelSettings.scaleMode;
		panelSettings2.referenceResolution = panelSettings.referenceResolution;
		panelSettings2.match = panelSettings.match;
		panelSettings2.sortingOrder = sortingOrder;
		return panelSettings2;
	}
}
