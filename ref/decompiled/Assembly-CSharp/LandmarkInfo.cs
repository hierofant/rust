using UnityEngine;

public class LandmarkInfo : MonoBehaviour
{
	[Header("LandmarkInfo")]
	public bool shouldDisplayOnMap;

	public bool isLayerSpecific;

	public Translate.Phrase displayPhrase;

	public Sprite mapIcon;

	public bool isDynamic;

	public virtual MapLayer MapLayer => MapLayer.Overworld;

	protected virtual void Awake()
	{
		if (!isDynamic && (bool)TerrainMeta.Path)
		{
			TerrainMeta.Path.Landmarks.Add(this);
		}
	}
}
