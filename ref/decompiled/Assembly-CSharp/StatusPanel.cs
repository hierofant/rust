using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanel : MonoBehaviour
{
	[SerializeField]
	[Header("On Status")]
	private RustText onStatusText;

	[SerializeField]
	private Image onStatusImage;

	[Header("Off Status")]
	[SerializeField]
	private RustText offStatusText;

	[SerializeField]
	private Image offStatusImage;

	public void SetStatus(bool status)
	{
		onStatusText.gameObject.SetActive(status);
		offStatusText.gameObject.SetActive(!status);
		onStatusImage.gameObject.SetActive(status);
		offStatusImage.gameObject.SetActive(!status);
	}
}
