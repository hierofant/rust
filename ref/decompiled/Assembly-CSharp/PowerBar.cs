using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour, IGameUIDisconnectCallback
{
	public static PowerBar Instance;

	public Image powerInner;

	public float fullSize;

	public CanvasGroup group;
}
