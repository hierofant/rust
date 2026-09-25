using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_ServerBrowser_TagModifierButton : RustButton
{
	[Header("Tag Modifier Button")]
	[SerializeField]
	private string _serverTag;

	[SerializeField]
	private RustText _countText;

	[SerializeField]
	private bool _secureTag;

	private string _tag;

	private LTDescr _countTween;

	public string ServerTag => _serverTag;

	public bool IsSecureTag => _secureTag;

	public string CompactTag
	{
		get
		{
			if (_tag == null)
			{
				_tag = ServerTagCompressor.ShortenTag(_serverTag);
			}
			return _tag;
		}
	}

	public void SetCount(int count)
	{
		if (!(_countText == null))
		{
			if (_countTween != null)
			{
				LeanTween.cancel(_countTween.id);
			}
			int.TryParse(_countText.text, out var result);
			_countTween = LeanTween.value(base.gameObject, result, count, 0.2f).setEaseOutQuad().setOnUpdate(delegate(float val)
			{
				int num = Mathf.RoundToInt(val);
				_countText.text = num.ToString();
			});
		}
	}
}
