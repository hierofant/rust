using UnityEngine;

public class CopyPasteDataAsset : BaseScriptableObject
{
	[SerializeField]
	private byte[] data;

	public void SetData(byte[] newData)
	{
		data = newData;
	}

	public byte[] GetData()
	{
		return data;
	}
}
