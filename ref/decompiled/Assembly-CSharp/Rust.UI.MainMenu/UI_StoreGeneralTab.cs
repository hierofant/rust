using System;
using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI.MainMenu;

public class UI_StoreGeneralTab : UI_StoreTabBase
{
	[Serializable]
	public struct Section
	{
		public string Name;

		public GameObject Group;

		public RectTransform MarkdownTarget;

		public RustButton Button;
	}

	[SerializeField]
	[Space]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransform content;

	[SerializeField]
	private FlexElement scrollRectContentFlex;

	[SerializeField]
	[Space]
	private Section[] sections;

	[SerializeField]
	private RectMask2D mask;
}
