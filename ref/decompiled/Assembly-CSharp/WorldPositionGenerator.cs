using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facepunch;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/WorldPositionGenerator")]
public class WorldPositionGenerator : ScriptableObject
{
	private struct InputValuesIdentifierData : IEquatable<InputValuesIdentifierData>
	{
		public Vector3 origin;

		public float minDist;

		public float maxDist;

		public InputValuesIdentifierData(Vector3 origin, float minDist, float maxDist)
		{
			this.origin = origin;
			this.minDist = minDist;
			this.maxDist = maxDist;
		}

		public bool Equals(InputValuesIdentifierData other)
		{
			if (origin == other.origin && Mathf.Approximately(minDist, other.minDist))
			{
				return Mathf.Approximately(maxDist, other.maxDist);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is InputValuesIdentifierData other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(origin, minDist, maxDist);
		}
	}

	private class PreprocessedData
	{
		public Rect[] preprocessedElementRects;
	}

	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public bool aboveWater;

	public float MaxSlopeRadius;

	public float MaxSlopeDegrees = 90f;

	public float CheckSphereRadius;

	public LayerMask CheckSphereMask;

	private Vector3 _origin;

	private Vector3 _area;

	private ByteQuadtree _quadtree = new ByteQuadtree();

	private Dictionary<InputValuesIdentifierData, PreprocessedData> _processedValuesCache = new Dictionary<InputValuesIdentifierData, PreprocessedData>();

	private bool isInitialized;

	private static WorldPositionGenerator precalculatePositionsInstance;

	private static int res;

	private static byte[] map;

	[ThreadStatic]
	private static float factor;

	private static Action<int, int> _actionSlopeCheck;

	private static bool isPrecalculating;

	private static Action<int, int> actionSlopeCheck => SlopeCheck;

	public bool TrySample(Vector3 origin, float minDist, float maxDist, float minDist_2x, float maxDist_2x, out Vector3 position)
	{
		BufferList<Rect> blockedRects;
		Rect[] potentialElementRects;
		int elementsToCheckCount;
		int endIdx;
		using (TimeWarning.New("WorldPositionGenerator.TrySample"))
		{
			position = Vector3.zero;
			if (!isInitialized)
			{
				PrecalculatePositions(this);
			}
			Rect inclusion = new Rect(origin.x - maxDist, origin.z - maxDist, maxDist_2x, maxDist_2x);
			Rect exclusion = new Rect(origin.x - minDist, origin.z - minDist, minDist_2x, minDist_2x);
			blockedRects = Pool.Get<BufferList<Rect>>();
			foreach (ListHashSet<Vector3> value2 in BaseMission.blockedPoints.Values)
			{
				for (int i = 0; i < value2.Count; i++)
				{
					Vector3 vector = value2[i];
					Rect element = new Rect(vector.x - 10f, vector.z - 10f, 20f, 20f);
					blockedRects.Add(element);
				}
			}
			bool result = false;
			List<ByteQuadtree.Element> elementsBuffer;
			if (_processedValuesCache.TryGetValue(new InputValuesIdentifierData(origin, minDist, maxDist), out var value))
			{
				potentialElementRects = value.preprocessedElementRects;
			}
			else
			{
				elementsBuffer = Pool.Get<List<ByteQuadtree.Element>>();
				List<Rect> obj = Pool.Get<List<Rect>>();
				elementsBuffer.Add(_quadtree.Root);
				for (int j = 0; j < elementsBuffer.Count; j++)
				{
					ByteQuadtree.Element element2 = elementsBuffer[j];
					if (element2.IsLeaf)
					{
						Rect elementRect = GetElementRect(element2);
						obj.Add(elementRect);
						continue;
					}
					elementsBuffer.RemoveUnordered(j--);
					EvaluateElement(element2.Child1);
					EvaluateElement(element2.Child2);
					EvaluateElement(element2.Child3);
					EvaluateElement(element2.Child4);
				}
				InputValuesIdentifierData key = new InputValuesIdentifierData(origin, minDist, maxDist);
				_processedValuesCache.Add(key, new PreprocessedData
				{
					preprocessedElementRects = obj.ToArray()
				});
				potentialElementRects = _processedValuesCache[key].preprocessedElementRects;
				Pool.FreeUnmanaged(ref elementsBuffer);
				Pool.FreeUnmanaged(ref obj);
				if (_processedValuesCache.Count > 16)
				{
					Debug.LogWarning(string.Format("{0} {1} added a new preprocessed values cache for input values origin: {2}, minDist: {3}, maxDist: {4} bringing total number of preprocessed value instances to {5}. ", "WorldPositionGenerator", base.name, origin, minDist, maxDist, _processedValuesCache.Count) + "This means that either this server either has many potential origin points for this WorldPositionGenerator, or the origin points are moving.");
				}
			}
			elementsToCheckCount = potentialElementRects.Length;
			endIdx = elementsToCheckCount;
			while (elementsToCheckCount > 0)
			{
				int num = UnityEngine.Random.Range(0, endIdx);
				Rect rect2 = potentialElementRects[num];
				if (IsCandidateValid(rect2, out var foundPosition2))
				{
					result = true;
					position = foundPosition2;
					break;
				}
				DiscardCandidate_End(rect2, num);
			}
			Pool.FreeUnmanaged(ref blockedRects);
			return result;
			void EvaluateElement(ByteQuadtree.Element child)
			{
				if (child.Value != 0)
				{
					Rect elementRect2 = GetElementRect(child);
					if (elementRect2.Overlaps(inclusion) && (!exclusion.Contains(elementRect2.min) || !exclusion.Contains(elementRect2.max)))
					{
						elementsBuffer.Add(child);
					}
				}
			}
		}
		void DiscardCandidate_End(Rect candidate, int candidateIdx)
		{
			endIdx--;
			Rect rect3 = potentialElementRects[endIdx];
			potentialElementRects[candidateIdx] = rect3;
			potentialElementRects[endIdx] = candidate;
			elementsToCheckCount--;
		}
		bool IsCandidateValid(Rect rect, out Vector3 foundPosition)
		{
			using (TimeWarning.New("WorldPositionGenerator.IsCandidateValid"))
			{
				foundPosition = Vector3.zero;
				if (blockedRects.Count > 0)
				{
					for (int k = 0; k < blockedRects.Count; k++)
					{
						Rect rect4 = blockedRects[k];
						if (rect4.Contains(rect.min) && rect4.Contains(rect.max))
						{
							return false;
						}
					}
				}
				if (CheckSphereRadius <= float.Epsilon)
				{
					foundPosition = (rect.min + rect.size * new Vector2(UnityEngine.Random.value, UnityEngine.Random.value)).XZ3D();
				}
				else
				{
					Vector3 vector2 = rect.center.XZ3D();
					vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
					if (Physics.CheckSphere(vector2, CheckSphereRadius, CheckSphereMask.value))
					{
						return false;
					}
					foundPosition = vector2;
				}
				foundPosition = foundPosition.WithY(aboveWater ? WaterLevel.GetWaterOrTerrainSurface(foundPosition, waves: false, volumes: false) : TerrainMeta.HeightMap.GetHeight(foundPosition));
				return BaseMission.PositionGenerator.TryAlignToGround(foundPosition, out foundPosition);
			}
		}
	}

	private Rect GetElementRect(ByteQuadtree.Element element)
	{
		int num = 1 << element.Depth;
		float num2 = 1f / (float)num;
		Vector2 vector = element.Coords * num2;
		return new Rect(_origin.x + vector.x * _area.x, _origin.z + vector.y * _area.z, _area.x * num2, _area.z * num2);
	}

	private static void SlopeCheck(int slopeX, int slopeZ)
	{
		if (TerrainMeta.HeightMap.GetSlope(slopeX, slopeZ) > precalculatePositionsInstance.MaxSlopeDegrees)
		{
			factor = 0f;
		}
	}

	public static void PrecalculatePositions(WorldPositionGenerator positionGenerator)
	{
		if (positionGenerator.isInitialized)
		{
			return;
		}
		if (isPrecalculating)
		{
			Debug.LogWarning("Attempted to precalculate positions for " + positionGenerator.name + " while already precalculating " + precalculatePositionsInstance.name);
			return;
		}
		precalculatePositionsInstance = positionGenerator;
		isPrecalculating = true;
		using (TimeWarning.New("WorldPositionGenerator.PrecalculatePositions"))
		{
			if (map == null)
			{
				res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
				map = new byte[res * res];
			}
			Parallel.For(0, res, delegate(int z)
			{
				for (int i = 0; i < res; i++)
				{
					float normX = ((float)i + 0.5f) / (float)res;
					float normZ = ((float)z + 0.5f) / (float)res;
					factor = precalculatePositionsInstance.Filter.GetFactor(normX, normZ);
					if (factor > 0f && precalculatePositionsInstance.MaxSlopeRadius > 0f)
					{
						TerrainMeta.HeightMap.ForEach(normX, normZ, precalculatePositionsInstance.MaxSlopeRadius / (float)res, actionSlopeCheck);
					}
					map[z * res + i] = (byte)((factor >= precalculatePositionsInstance.FilterCutoff) ? (255f * factor) : 0f);
				}
			});
			precalculatePositionsInstance._origin = TerrainMeta.Position;
			precalculatePositionsInstance._area = TerrainMeta.Size;
			byte[] baseValues = map.ToArray();
			precalculatePositionsInstance._quadtree.UpdateValues(baseValues);
			precalculatePositionsInstance.isInitialized = true;
			isPrecalculating = false;
		}
	}
}
