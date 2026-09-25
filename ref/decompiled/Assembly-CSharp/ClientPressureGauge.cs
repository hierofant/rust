using System.Collections.Generic;
using UnityEngine;

public class ClientPressureGauge : FacepunchBehaviour, IClientComponent
{
	public List<Transform> pressureGaugeHandles;

	public Vector2 pressureGaugeRotationRange = new Vector2(-135f, 135f);
}
