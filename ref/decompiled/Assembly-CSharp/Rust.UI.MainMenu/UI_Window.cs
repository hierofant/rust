using System;
using Facepunch.Flexbox;
using UnityEngine;

namespace Rust.UI.MainMenu;

public class UI_Window : BaseMonoBehaviour
{
	[SerializeField]
	private bool _skipAutoFixState;

	[SerializeField]
	[Header("Window - Transitions")]
	private FlexTransition _openTransition;

	[SerializeField]
	private bool _oneShotTransition;

	[Header("Window - Components")]
	[SerializeField]
	protected CanvasGroup _group;

	[SerializeField]
	protected UIEscapeCapture _escape;

	[SerializeField]
	protected FlexElement _flex;

	protected bool _firstTimeOpened = true;

	protected bool _opened;

	public event Action OnOpen;

	public event Action OnClose;

	protected virtual void Awake()
	{
		if (!_opened && !_skipAutoFixState)
		{
			FixBrokenState();
		}
	}

	private void FixBrokenState()
	{
		using (TimeWarning.New("UI_Window.FixBrokenState"))
		{
			if (_group == null)
			{
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: false);
				}
				return;
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			SetUI(state: false);
		}
	}

	[UnityEvent]
	public virtual void Open()
	{
		if (!_opened)
		{
			_opened = true;
			SetUI(state: true);
			OnOpened();
			if (_firstTimeOpened)
			{
				_firstTimeOpened = false;
			}
		}
	}

	[UnityEvent]
	public virtual void Close()
	{
		if (_opened)
		{
			_opened = false;
			SetUI(state: false);
			OnClosed();
		}
	}

	public bool IsOpen()
	{
		return _opened;
	}

	protected virtual void OnOpened()
	{
		this.OnOpen?.Invoke();
		if (_openTransition != null)
		{
			if (_oneShotTransition)
			{
				_openTransition.PlayOneOff();
			}
			else
			{
				_openTransition.SwitchState(enabled: true, animate: true);
			}
		}
	}

	protected virtual void OnClosed()
	{
		this.OnClose?.Invoke();
		if ((bool)_openTransition)
		{
			_openTransition.SwitchState(enabled: false, animate: false);
		}
	}

	public virtual void SetUI(bool state)
	{
		if (_group == null)
		{
			base.gameObject.SetActive(state);
		}
		else
		{
			if (state && !base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			_group.alpha = (state ? 1 : 0);
			_group.interactable = state;
			_group.blocksRaycasts = state;
		}
		if (_escape != null)
		{
			_escape.enabled = state;
		}
		if (_flex != null)
		{
			_flex.enabled = state;
		}
	}
}
