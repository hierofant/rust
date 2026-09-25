using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

public class NoticeArea : SingletonComponent<NoticeArea>
{
	public Canvas canvas;

	public GameObjectRef itemPickupPrefab;

	public GameObjectRef itemPickupCondensedText;

	public GameObjectRef itemDroppedPrefab;

	public AnimationCurve pickupSizeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve pickupAlphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve reuseAlphaCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve reuseSizeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private List<IVitalNotice> notices = new List<IVitalNotice>();

	public GameObject CustomVitalPrefab;

	private CustomVitals customVitalsData;

	private List<VitalNote> customVitalsObjects = new List<VitalNote>();

	protected override void Awake()
	{
		base.Awake();
		notices.Clear();
		GetComponentsInChildren(includeInactive: true, notices);
	}
}
