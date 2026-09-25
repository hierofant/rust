using System;
using UnityEngine;

namespace Rust.UI.MainMenu;

[Serializable]
public class UI_MainMenuNavigationEntry
{
	public string Path;

	public GameObject Reference;

	[NonSerialized]
	public UI_Page Page;

	public void Hide()
	{
		if (CheckReference() && Page != null)
		{
			Page.Close();
		}
	}

	public void Show()
	{
		if (CheckReference() && Page != null)
		{
			Page.Open();
		}
	}

	private bool CheckReference()
	{
		if (Reference == null)
		{
			Debug.LogError("Navigation Entry '" + Path + "' doesn't have a valid reference.");
			return false;
		}
		return true;
	}
}
