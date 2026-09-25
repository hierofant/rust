using System;
using UnityEngine;

public class DartsGameViewModel : BaseViewModel, IAnimationEventReceiver
{
	private static readonly int DETAIL_COLOR = Shader.PropertyToID("_DetailColor");

	[Header("Dependency References")]
	public Transform dartThrowBone;

	public Transform dartVisualRoot;

	public Renderer meshRenderer;

	public Color player1Colour;

	public Color player2Colour;

	public DartsGameViewModelComponent viewModelComponent;

	[NonSerialized]
	public DartsGameMountable mountable;

	[NonSerialized]
	public DartsGameBoard Board;
}
