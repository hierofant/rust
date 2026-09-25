using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ZoomImage : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private float _minimumScale = 0.5f;

	[SerializeField]
	private float _initialScale = 1f;

	[SerializeField]
	private float _maximumScale = 3f;

	[SerializeField]
	private float _scaleIncrement = 0.5f;

	[HideInInspector]
	private Vector3 _scale;

	private RectTransform _thisTransform;

	public float MinimumScale => _minimumScale;

	public float MaximumScale => _maximumScale;

	public float CurrentScale => base.transform.localScale.x;

	public float NormalizedZoom
	{
		get
		{
			if (_maximumScale <= _minimumScale)
			{
				return 0f;
			}
			return Mathf.Clamp01((CurrentScale - _minimumScale) / (_maximumScale - _minimumScale));
		}
	}

	private void Awake()
	{
		_thisTransform = base.transform as RectTransform;
		_scale.Set(_initialScale, _initialScale, 1f);
		_thisTransform.localScale = _scale;
	}

	public void OnScroll(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_thisTransform, Mouse.current.position.ReadValue(), null, out var localPoint);
		float y = eventData.scrollDelta.y;
		if (y > 0f && _scale.x < _maximumScale)
		{
			_scale.Set(_scale.x + _scaleIncrement, _scale.y + _scaleIncrement, 1f);
			_thisTransform.localScale = _scale;
			_thisTransform.anchoredPosition -= localPoint * _scaleIncrement;
		}
		else if (y < 0f && _scale.x > _minimumScale)
		{
			_scale.Set(_scale.x - _scaleIncrement, _scale.y - _scaleIncrement, 1f);
			_thisTransform.localScale = _scale;
			_thisTransform.anchoredPosition += localPoint * _scaleIncrement;
		}
	}
}
