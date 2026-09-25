using Rust.UI;
using TMPro;
using UnityEngine;

namespace Facepunch.UI;

public class ESPPlayerInfo : MonoBehaviour
{
	public Vector3 WorldOffset;

	public RustText Text;

	public GameObject Dot;

	public TextMeshProUGUI[] TextElements;

	public RustIcon Loading;

	public RustIcon VoipIcon;

	public GameObject ClanElement;

	public RustText ClanText;

	public CanvasGroup group;

	public QueryVis visCheck;

	public BasePlayer Entity { get; set; }
}
