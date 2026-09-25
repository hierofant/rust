using System;
using UnityEngine;

public class ItemModGenericInfo : MonoBehaviour
{
	[Serializable]
	public class ItemModInfoEntry
	{
		public Translate.Phrase phrase;

		public string value;

		public virtual string GetValueString()
		{
			return value;
		}
	}

	public ItemModInfoEntry[] infoEntries;
}
