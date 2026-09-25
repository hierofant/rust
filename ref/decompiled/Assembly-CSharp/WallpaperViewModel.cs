using UnityEngine;

public class WallpaperViewModel : MonoBehaviour
{
	public GameObject[] models;

	public void ToggleModels(int mode)
	{
		for (int i = 0; i < models.Length; i++)
		{
			models[i].SetActive(mode - 1 == i);
		}
	}
}
