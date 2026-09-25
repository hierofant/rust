using UnityEngine;

public class PaintingFavouriteColours : SingletonComponent<PaintingFavouriteColours>
{
	public MeshPaintController meshPaintController;

	public GameObject panelIcon;

	public PaintingFavouriteColorButton buttonPrefab;

	public Transform buttonContainer;

	public GameObject addColourButton;
}
