using UnityEngine;

public class PreventBuildingMonumentTag : MonoBehaviour
{
	public bool autoFindMonument;

	[SerializeField]
	private MonumentInfo AttachedMonument;

	private static readonly ListHashSet<PreventBuildingMonumentTag> allTags = new ListHashSet<PreventBuildingMonumentTag>();

	private Collider volume;

	private bool hasVolume;

	public static ListHashSet<PreventBuildingMonumentTag> All => allTags;

	private void Awake()
	{
		allTags.TryAdd(this);
	}

	private void OnDestroy()
	{
		allTags.Remove(this);
	}

	public bool TryGetVolume(out OBB result)
	{
		if (!hasVolume)
		{
			volume = GetComponent<Collider>();
			hasVolume = true;
		}
		if (volume == null)
		{
			result = default(OBB);
			return false;
		}
		Transform transform = volume.transform;
		if (volume is BoxCollider boxCollider)
		{
			result = new OBB(transform, new Bounds(boxCollider.center, boxCollider.size));
			return true;
		}
		if (volume is SphereCollider sphereCollider)
		{
			Vector3 lossyScale = transform.lossyScale;
			float num = sphereCollider.radius * 2f * Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
			result = new OBB(transform.TransformPoint(sphereCollider.center), Vector3.one * num, Quaternion.identity);
			return true;
		}
		if (volume is CapsuleCollider capsuleCollider)
		{
			float num2 = capsuleCollider.radius * 2f;
			Vector3 size = new Vector3(num2, num2, num2);
			switch (capsuleCollider.direction)
			{
			case 0:
				size.x = Mathf.Max(capsuleCollider.height, num2);
				break;
			case 1:
				size.y = Mathf.Max(capsuleCollider.height, num2);
				break;
			default:
				size.z = Mathf.Max(capsuleCollider.height, num2);
				break;
			}
			result = new OBB(transform, new Bounds(capsuleCollider.center, size));
			return true;
		}
		Bounds bounds = volume.bounds;
		result = new OBB(bounds.center, bounds.size, Quaternion.identity);
		return bounds.size != Vector3.zero;
	}

	public MonumentInfo GetAttachedMonument()
	{
		if (autoFindMonument && AttachedMonument == null)
		{
			MonumentInfo attachedMonument = TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, base.transform.position);
			AttachedMonument = attachedMonument;
		}
		return AttachedMonument;
	}

	public void SetMonument(MonumentInfo monument)
	{
		AttachedMonument = monument;
	}
}
