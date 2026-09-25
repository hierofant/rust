using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_TagController : MonoBehaviour
{
	[Serializable]
	public class TagData
	{
		public string TagName;

		public GameObject TagObject;
	}

	[SerializeField]
	private List<TagData> _tags = new List<TagData>();

	public void ClearTags()
	{
		foreach (TagData tag in _tags)
		{
			if (tag.TagObject.activeInHierarchy)
			{
				tag.TagObject.SetActive(value: false);
			}
		}
	}

	public void EnableTag(string tagName)
	{
		foreach (TagData tag in _tags)
		{
			if (tag.TagName == tagName && !tag.TagObject.activeInHierarchy)
			{
				tag.TagObject.SetActive(value: true);
				return;
			}
		}
		Debug.LogError("Tag '" + tagName + "' not found in the list.");
	}
}
