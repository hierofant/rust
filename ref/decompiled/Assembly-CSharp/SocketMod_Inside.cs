using UnityEngine;

public class SocketMod_Inside : SocketMod
{
	public bool wantsInside = true;

	public bool customDirections;

	public Vector3[] customRayDirections;

	private static readonly Vector3[] outsideLookupDirs = new Vector3[8]
	{
		new Vector3(1f, 1f, 0f).normalized,
		new Vector3(0f, -1f, 0f).normalized,
		new Vector3(0f, 1f, 1f).normalized,
		new Vector3(-1f, 1f, 0f).normalized,
		new Vector3(0f, 0f, 1f).normalized,
		new Vector3(0f, 1f, 0f).normalized,
		new Vector3(1f, 0f, 0.5f).normalized,
		new Vector3(-1f, 0f, 0.5f).normalized
	};

	protected override Translate.Phrase ErrorPhrase
	{
		get
		{
			if (!wantsInside)
			{
				return ConstructionErrors.WantsOutside;
			}
			return ConstructionErrors.WantsInside;
		}
	}

	private Vector3[] directions
	{
		get
		{
			if (!customDirections)
			{
				return outsideLookupDirs;
			}
			return customRayDirections;
		}
	}

	public override bool DoCheck(ref Construction.Placement place)
	{
		Vector3 vector = place.position + place.rotation * baseSocket.localPosition;
		Quaternion quaternion = place.rotation * baseSocket.localRotation;
		Vector3 pos = vector + quaternion * localPosition;
		Quaternion rotation = quaternion * localRotation;
		bool flag = IsOutside(pos, rotation, directions);
		return !wantsInside == flag;
	}

	public static bool IsOutside(Vector3 pos, Transform tr, int layerMask = 136380416)
	{
		return IsOutside(pos, tr.rotation, outsideLookupDirs, layerMask);
	}

	public static bool IsOutside(Vector3 pos, Transform tr, Vector3[] dirs, int layerMask = 136380416)
	{
		return IsOutside(pos, tr.rotation, dirs, layerMask);
	}

	public static bool IsOutside(Vector3 pos, Quaternion rotation, Vector3[] dirs, int layerMask = 136380416)
	{
		using (TimeWarning.New("SocketMod_Inside.IsOutside"))
		{
			float num = 20f;
			int num2 = 0;
			bool flag = true;
			for (int i = 0; i < dirs.Length; i++)
			{
				Vector3 direction = rotation * dirs[i];
				if (Physics.Raycast(new Ray(pos, direction), out var hitInfo, num - 0.5f, layerMask))
				{
					if (GameObjectEx.IsOnLayers(hitInfo.collider.gameObject, layerMask))
					{
						num2++;
					}
				}
				else
				{
					flag = false;
				}
			}
			if (flag)
			{
				return num2 < 2;
			}
			return true;
		}
	}
}
