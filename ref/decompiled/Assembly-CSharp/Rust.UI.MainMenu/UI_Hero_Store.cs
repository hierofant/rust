using System.Collections.Generic;
using Facepunch.Flexbox;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Rust.UI.MainMenu;

public class UI_Hero_Store : UI_Hero_InfoBox
{
	[SerializeField]
	[Header("Hero Store")]
	private VideoPlayer _video;

	[SerializeField]
	private GameObject _titleContainer;

	[SerializeField]
	private FlexTransition _loadingFlex;

	[SerializeField]
	private CanvasGroup _loadingGroup;

	[SerializeField]
	private RawImage _videoImage;

	[SerializeField]
	private List<Image> _progressBars;

	[SerializeField]
	private List<Image> _progressBarHolders;

	[Header("On Hover Animation")]
	[SerializeField]
	private CanvasGroup _menuBlurGroup;

	[SerializeField]
	private GameObject _expandButton;

	[SerializeField]
	private GameObject _viewInStoreButton;
}
