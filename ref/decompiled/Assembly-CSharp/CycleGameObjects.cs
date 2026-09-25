using UnityEngine;

public class CycleGameObjects : MonoBehaviour, IClientComponent
{
	[SerializeField]
	private GameObject[] objects;

	[SerializeField]
	private float interval = 2f;

	[SerializeField]
	private bool shuffle;
}
