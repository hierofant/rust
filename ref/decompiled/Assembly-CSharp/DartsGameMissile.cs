using System;
using UnityEngine;

public class DartsGameMissile : FacepunchBehaviour
{
	private static readonly int DETAIL_COLOR = Shader.PropertyToID("_DetailColor");

	[Tooltip("Flight curve: start and end points must remain at 0. Any change in the curve will correspond to the y-height of the dart during its flight.")]
	public AnimationCurve flightLiftCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

	public Transform dartTipTransform;

	public Renderer meshRenderer;

	public Color player1Colour;

	public Color player2Colour;

	[NonSerialized]
	public Vector3 hitLocalPosition;

	[Tooltip("How long (in seconds) the dart takes to travel to the board.")]
	public float flightDuration = 0.3f;

	[Header("Audio/Effects")]
	public SoundDefinition throwSound;

	public GameObjectRef hitEffect;

	public DartsGameBoard Board { get; private set; }
}
