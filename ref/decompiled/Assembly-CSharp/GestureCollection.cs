using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Gestures/Gesture Collection")]
public class GestureCollection : BaseScriptableObject
{
	public static uint HeavyLandingId = 3204230781u;

	private static GestureCollection _instance = null;

	public GestureConfig[] AllGestures;

	public float GestureVmInDuration = 0.25f;

	public AnimationCurve GestureInCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float GestureVmOutDuration = 0.25f;

	public AnimationCurve GestureOutCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float GestureViewmodelDeployDelay = 0.25f;

	public Sprite EmptyGestureSlotSprite;

	public Translate.Phrase EmptySlotTitle;

	public Translate.Phrase EmptySlotDescription;

	private Dictionary<uint, GestureConfig> _idToGestureLookup;

	private Dictionary<string, GestureConfig> _convarToGestureLookup;

	public static GestureCollection Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<GestureCollection>("assets/prefabs/player/gestures/gesturecollection.asset");
			}
			return _instance;
		}
	}

	private Dictionary<uint, GestureConfig> idToGestureLookup
	{
		get
		{
			if (_idToGestureLookup == null)
			{
				_idToGestureLookup = new Dictionary<uint, GestureConfig>();
				GestureConfig[] allGestures = AllGestures;
				foreach (GestureConfig gestureConfig in allGestures)
				{
					_idToGestureLookup.Add(gestureConfig.gestureId, gestureConfig);
				}
			}
			return _idToGestureLookup;
		}
	}

	private Dictionary<string, GestureConfig> convarToGestureLookup
	{
		get
		{
			if (_convarToGestureLookup == null)
			{
				_convarToGestureLookup = new Dictionary<string, GestureConfig>();
				GestureConfig[] allGestures = AllGestures;
				foreach (GestureConfig gestureConfig in allGestures)
				{
					_convarToGestureLookup.Add(gestureConfig.convarName, gestureConfig);
				}
			}
			return _convarToGestureLookup;
		}
	}

	public GestureConfig IdToGesture(uint id)
	{
		return idToGestureLookup.GetValueOrDefault(id, null);
	}

	public GestureConfig GestureConvarNameToGesture(string gestureName)
	{
		return convarToGestureLookup.GetValueOrDefault(gestureName, null);
	}
}
