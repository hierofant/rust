using System;
using Rust.UI;
using UnityEngine;

public class GenericInformationPanel : ItemInformationPanel
{
	[Serializable]
	private struct Entry
	{
		public RustText label;

		public ItemTextValue value;

		public Tooltip tooltip;
	}

	[SerializeField]
	private Entry[] entries;
}
