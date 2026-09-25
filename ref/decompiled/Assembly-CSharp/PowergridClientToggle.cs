using System;
using UnityEngine;
using UnityEngine.Events;

public class PowergridClientToggle : FacepunchBehaviour, IClientComponent
{
	[Serializable]
	private class StageChangeEvent
	{
		public int requiredPowergridStage;

		public UnityEvent onPowergridStageReached = new UnityEvent();

		public UnityEvent onPowergridStageLost = new UnityEvent();
	}

	public int requiredPowergridStage = 1;

	[SerializeField]
	private UnityEvent onPowergridStageReached = new UnityEvent();

	[SerializeField]
	private UnityEvent onPowergridStageLost = new UnityEvent();

	[SerializeField]
	private StageChangeEvent[] expandedStageChangeEvents = Array.Empty<StageChangeEvent>();
}
