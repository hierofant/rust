using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SafeVideoPlayer : MonoBehaviour
{
	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private RawImage _outputImage;

	[SerializeField]
	private string primaryURL;

	[SerializeField]
	private string fallbackURL;
}
