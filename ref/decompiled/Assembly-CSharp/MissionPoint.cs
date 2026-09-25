using System.Collections.Generic;
using Rust;
using UnityEngine;

public class MissionPoint : MonoBehaviour
{
	public enum MissionPointEnum
	{
		EasyMonument = 1,
		MediumMonument = 2,
		HardMonument = 4,
		Item_Hidespot = 8,
		Underwater = 0x80,
		Tutorial_Bear = 0x100,
		AtmosphereSensor = 0x200,
		SafeZoneMonument = 0x400,
		OilRigMonument = 0x800,
		OilRigCCTVRoom = 0x1000,
		FloatingCity = 0x2000
	}

	public bool dropToGround = true;

	private const float BLOCK_DISTANCE_THRESHOLD = 3f;

	private const float BLOCK_DISTANCE_THRESHOLD_SQUARED = 9f;

	public const int EVERYTHING = -1;

	public const int NOTHING = 0;

	public const int EASY_MONUMENT = 1;

	public const int MED_MONUMENT = 2;

	public const int HARD_MONUMENT = 4;

	public const int ITEM_HIDESPOT = 8;

	public const int UNDERWATER = 128;

	public const int TUTORIAL_BEAR = 256;

	public const int ATMOSPHERE_SENSOR = 512;

	public const int SAFE_ZONE_MONUMENT = 1024;

	public const int OILRIG_MONUMENT = 2048;

	public const int OILRIG_CCTVROOM = 4096;

	public const int FLOATINGCITY = 8192;

	public const int EASY_MONUMENT_IDX = 0;

	public const int MED_MONUMENT_IDX = 1;

	public const int HARD_MONUMENT_IDX = 2;

	public const int ITEM_HIDESPOT_IDX = 3;

	public const int FOREST_IDX = 4;

	public const int ROADSIDE_IDX = 5;

	public const int BEACH = 6;

	public const int UNDERWATER_IDX = 7;

	public const int TUTORIAL_BEAR_IDX = 8;

	public const int ATMOSPHERE_SENSOR_IDX = 9;

	public const int SAFE_ZONE_MONUMENT_IDX = 10;

	public const int OILRIG_MONUMENT_IDX = 11;

	public const int OILRIG_CCTVROOM_IDX = 12;

	public const int FLOATINGCITY_IDX = 13;

	private static Dictionary<int, int> type2index = new Dictionary<int, int>
	{
		{ 1, 0 },
		{ 2, 1 },
		{ 4, 2 },
		{ 8, 3 },
		{ 128, 7 },
		{ 256, 8 },
		{ 512, 9 },
		{ 1024, 10 },
		{ 2048, 11 },
		{ 4096, 12 },
		{ 8192, 13 }
	};

	[InspectorFlags]
	public MissionPointEnum Flags = (MissionPointEnum)(-1);

	public static ListHashSet<MissionPoint> server_allMissionPoints = new ListHashSet<MissionPoint>();

	public static int TypeToIndex(int id)
	{
		return type2index[id];
	}

	public static int IndexToType(int idx)
	{
		return 1 << idx;
	}

	public void OnEnable()
	{
		server_allMissionPoints.TryAdd(this);
	}

	private void Start()
	{
		if (dropToGround)
		{
			SingletonComponent<InvokeHandler>.Instance.Invoke(DropToGround, 0.5f);
		}
	}

	private void DropToGround()
	{
		if ((bool)this)
		{
			if (Rust.Application.isLoading)
			{
				SingletonComponent<InvokeHandler>.Instance.Invoke(DropToGround, 0.5f);
			}
			else
			{
				TransformEx.DropToGround(base.transform);
			}
		}
	}

	public void OnDisable()
	{
		server_allMissionPoints.Remove(this);
	}

	public static bool GetMissionPoints(ref List<Vector3> points, Vector3 near, float minDistanceSqr, float maxDistanceSqr, int flags, int exclusionFlags)
	{
		using (TimeWarning.New("MissionPoint.GetMissionPoints"))
		{
			bool result = false;
			for (int i = 0; i < server_allMissionPoints.Count; i++)
			{
				MissionPoint missionPoint = server_allMissionPoints[i];
				MissionPointEnum flags2 = missionPoint.Flags;
				Vector3 position = missionPoint.transform.position;
				if (((uint)flags2 & (uint)flags) != (uint)flags || (exclusionFlags != 0 && ((uint)flags2 & (uint)exclusionFlags) != 0))
				{
					continue;
				}
				float num = Vector3.SqrMagnitude(position - near);
				if (!(num <= maxDistanceSqr) || !(num > minDistanceSqr))
				{
					continue;
				}
				if (BaseMission.blockedPoints.Count > 0)
				{
					bool flag = false;
					foreach (ListHashSet<Vector3> value in BaseMission.blockedPoints.Values)
					{
						for (int j = 0; j < value.Count; j++)
						{
							if (Vector3.SqrMagnitude(value[j] - position) < 9f)
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				points.Add(position);
				result = true;
			}
			return result;
		}
	}
}
