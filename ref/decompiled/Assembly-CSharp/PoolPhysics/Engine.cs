using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace PoolPhysics;

public class Engine : Pool.IPooled
{
	public struct RaycastHit
	{
		public Vector2 Point;

		public Vector2 Normal;

		public float Distance;

		public int BallId;

		public int WallIndex;
	}

	public Action<int> OnBallPocketed;

	public Action<Vector2, float> OnBallCollision;

	public Action<Vector2, float> OnWallCollision;

	public float DragConstant = 0.55f;

	private BufferList<Data.Ball> balls;

	private BufferList<Data.Wall> walls;

	private BufferList<Data.Pocket> pockets;

	private Dictionary<int, int> ballIndexLookup;

	private bool hasInitialised;

	public const float StopThresholdSqr = 0.005f;

	private const float MaxSubstepTravelFraction = 0.5f;

	private const int MaxSubsteps = 32;

	public BufferList<Data.Ball> Balls => balls;

	public BufferList<Data.Wall> Walls => walls;

	public BufferList<Data.Pocket> Pockets => pockets;

	public bool IsReady => hasInitialised;

	public Engine()
	{
		Setup();
	}

	~Engine()
	{
		Teardown();
	}

	private void Setup()
	{
		if (balls == null)
		{
			balls = Pool.Get<BufferList<Data.Ball>>();
		}
		if (walls == null)
		{
			walls = Pool.Get<BufferList<Data.Wall>>();
		}
		if (pockets == null)
		{
			pockets = Pool.Get<BufferList<Data.Pocket>>();
		}
		ballIndexLookup = Pool.Get<Dictionary<int, int>>();
		hasInitialised = true;
	}

	private void Teardown()
	{
		if (hasInitialised)
		{
			if (balls != null)
			{
				balls.Clear();
			}
			if (walls != null)
			{
				walls.Clear();
			}
			if (pockets != null)
			{
				pockets.Clear();
			}
			if (ballIndexLookup != null)
			{
				ballIndexLookup.Clear();
			}
			if (balls != null)
			{
				Pool.FreeUnmanaged(ref balls);
			}
			if (walls != null)
			{
				Pool.FreeUnmanaged(ref walls);
			}
			if (pockets != null)
			{
				Pool.FreeUnmanaged(ref pockets);
			}
			if (ballIndexLookup != null)
			{
				Pool.FreeUnmanaged(ref ballIndexLookup);
			}
		}
	}

	public void EnterPool()
	{
		Teardown();
	}

	public void LeavePool()
	{
		Setup();
	}

	public void AddBall(Data.Ball ball)
	{
		if (hasInitialised && !ballIndexLookup.ContainsKey(ball.Id))
		{
			ballIndexLookup[ball.Id] = balls.Count;
			balls.Add(ball);
		}
	}

	public void AddWall(Data.Wall wall)
	{
		if (hasInitialised)
		{
			walls.Add(wall);
		}
	}

	public void AddPocket(Data.Pocket pocket)
	{
		if (hasInitialised)
		{
			pockets.Add(pocket);
		}
	}

	public void RemoveBall(int id)
	{
		if (hasInitialised)
		{
			if (!ballIndexLookup.TryGetValue(id, out var value))
			{
				Log("Can't get ball with id " + id + " - not found");
				return;
			}
			balls.RemoveAt(value);
			ballIndexLookup.Remove(id);
			RebuildBallIndexLookup();
		}
	}

	public Data.Ball GetBall(int id)
	{
		if (!hasInitialised)
		{
			return default(Data.Ball);
		}
		if (!ballIndexLookup.TryGetValue(id, out var value))
		{
			Log("Can't get ball with id " + id + " - not found");
			return default(Data.Ball);
		}
		return balls[value];
	}

	public void SetBallPosition(int id, Vector2 pos)
	{
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball value2 = balls[value];
			value2.Position = pos;
			value2.Velocity = Vector2.zero;
			balls[value] = value2;
		}
	}

	public void SetBallVelocity(int id, Vector2 vel)
	{
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball value2 = balls[value];
			value2.Velocity = vel;
			balls[value] = value2;
		}
	}

	public void SetBallIsKinematic(int id, bool isKinematic)
	{
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball value2 = balls[value];
			value2.IsKinematic = isKinematic;
			if (isKinematic)
			{
				value2.Velocity = Vector2.zero;
			}
			balls[value] = value2;
		}
	}

	public void ApplyForce(int id, Vector2 force)
	{
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball value2 = balls[value];
			value2.Velocity += force;
			balls[value] = value2;
		}
	}

	public bool HasMovingBalls()
	{
		foreach (Data.Ball ball in balls)
		{
			if (!ball.IsKinematic && ball.Velocity.sqrMagnitude > 0.005f)
			{
				return true;
			}
		}
		return false;
	}

	public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance, out RaycastHit hit, int ignoreBallId = -1, bool includeBalls = true, bool includeWalls = true)
	{
		hit = default(RaycastHit);
		if (!hasInitialised)
		{
			return false;
		}
		if (maxDistance <= 0f)
		{
			return false;
		}
		if (direction.sqrMagnitude <= Mathf.Epsilon)
		{
			return false;
		}
		direction.Normalize();
		bool result = false;
		float num = maxDistance;
		if (includeWalls)
		{
			for (int i = 0; i < walls.Count; i++)
			{
				if (IntersectRaySegment(origin, direction, walls[i].A, walls[i].B, out var distance) && !(distance < 0f) && !(distance > num))
				{
					num = distance;
					result = true;
					hit.Point = origin + direction * distance;
					hit.Normal = walls[i].Normal;
					hit.Distance = distance;
					hit.BallId = -1;
					hit.WallIndex = i;
				}
			}
		}
		if (includeBalls)
		{
			for (int j = 0; j < balls.Count; j++)
			{
				Data.Ball ball = balls[j];
				if (!ball.IsKinematic && ball.Id != ignoreBallId && IntersectRayCircle(origin, direction, ball.Position, ball.Radius, out var distance2) && !(distance2 < 0f) && !(distance2 > num))
				{
					Vector2 vector = origin + direction * distance2;
					Vector2 normalized = (vector - ball.Position).normalized;
					num = distance2;
					result = true;
					hit.Point = vector;
					hit.Normal = normalized;
					hit.Distance = distance2;
					hit.BallId = ball.Id;
					hit.WallIndex = -1;
				}
			}
		}
		return result;
	}

	public void Tick(float delta)
	{
		if (!hasInitialised || balls.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PoolPhysics.Engine.Tick"))
		{
			int num = ComputeSubsteps(delta);
			float delta2 = delta / (float)num;
			for (int i = 0; i < num; i++)
			{
				IntegratePositions(delta2);
				ResolveWallCollisions();
				ResolveBallCollisions();
				ResolvePockets();
			}
			ApplyDrag(delta);
		}
	}

	private int ComputeSubsteps(float delta)
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ComputeSubsteps"))
		{
			float num = 0f;
			float num2 = float.MaxValue;
			for (int i = 0; i < balls.Count; i++)
			{
				if (!balls[i].IsKinematic)
				{
					float sqrMagnitude = balls[i].Velocity.sqrMagnitude;
					if (sqrMagnitude > num)
					{
						num = sqrMagnitude;
					}
					if (balls[i].Radius < num2)
					{
						num2 = balls[i].Radius;
					}
				}
			}
			if (num <= 0f || num2 == float.MaxValue || num2 <= 0f)
			{
				return 1;
			}
			float num3 = Mathf.Sqrt(num) * delta;
			float num4 = num2 * 0.5f;
			return Mathf.Clamp(Mathf.CeilToInt(num3 / num4), 1, 32);
		}
	}

	private void IntegratePositions(float delta)
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.IntegratePositions"))
		{
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball value = balls[i];
				if (!value.IsKinematic)
				{
					value.Position += value.Velocity * delta;
					balls[i] = value;
				}
			}
		}
	}

	private void ResolveWallCollisions()
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolveWallCollisions"))
		{
			if (!hasInitialised || walls.Count == 0)
			{
				return;
			}
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball value = balls[i];
				if (value.IsKinematic)
				{
					continue;
				}
				for (int j = 0; j < walls.Count; j++)
				{
					Data.Wall wall = walls[j];
					Vector2 vector = ClosestPointOnSegment(Balls[i].Position, Walls[j].A, Walls[j].B);
					if ((Balls[i].Position - vector).magnitude < value.Radius)
					{
						float num = Vector2.Dot(value.Position - vector, wall.Normal);
						value.Position += wall.Normal * (value.Radius - num);
						if (Vector2.Dot(value.Velocity, wall.Normal) < 0f)
						{
							value.Velocity = Vector2.Reflect(value.Velocity, wall.Normal);
							value.Velocity *= 0.85f;
							float arg = Vector2.Dot(value.Velocity, wall.Normal);
							OnWallCollision?.Invoke(value.Position, arg);
						}
						balls[i] = value;
					}
				}
			}
		}
	}

	private void ApplyDrag(float delta)
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ApplyDrag"))
		{
			if (!hasInitialised)
			{
				return;
			}
			float num = DeterministicExp((0f - DragConstant) * delta);
			foreach (Data.Ball ball in balls)
			{
				if (!ball.IsKinematic)
				{
					Vector2 vector = ball.Velocity * num;
					SetBallVelocity(ball.Id, (vector.sqrMagnitude > 0.005f) ? vector : Vector2.zero);
				}
			}
		}
	}

	private static float DeterministicExp(float x)
	{
		return 1f + x * (1f + x * (0.5f + x * (1f / 6f + x * (1f / 24f))));
	}

	private void ResolveBallCollisions()
	{
		if (!hasInitialised || balls.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolveBallCollisions"))
		{
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball value = balls[i];
				if (value.IsKinematic)
				{
					continue;
				}
				for (int j = 0; j < balls.Count; j++)
				{
					if (i == j)
					{
						continue;
					}
					Data.Ball value2 = balls[j];
					if (value2.IsKinematic)
					{
						continue;
					}
					Vector3 vector = value2.Position - value.Position;
					float magnitude = vector.magnitude;
					float num = value.Radius + value2.Radius;
					if (num > magnitude)
					{
						float num2 = num - magnitude;
						Vector2 vector2 = vector.normalized;
						Vector2 vector3 = vector2 * num2 / 2f;
						value.Position -= vector3;
						value2.Position += vector3;
						float num3 = Vector2.Dot(value.Velocity - value2.Velocity, vector2);
						if (num3 > 0f)
						{
							value.Velocity -= num3 * vector2;
							value2.Velocity += num3 * vector2;
							OnBallCollision?.Invoke((value.Position + value2.Position) * 0.5f, num3);
						}
						balls[i] = value;
						balls[j] = value2;
					}
				}
			}
		}
	}

	private void ResolvePockets()
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolvePockets"))
		{
			if (pockets.Count == 0)
			{
				return;
			}
			for (int num = balls.Count - 1; num >= 0; num--)
			{
				Data.Ball ball = balls[num];
				if (!ball.IsKinematic)
				{
					for (int i = 0; i < pockets.Count; i++)
					{
						Data.Pocket pocket = pockets[i];
						if ((ball.Position - pocket.Position).sqrMagnitude < pocket.Radius * pocket.Radius)
						{
							OnBallPocketed?.Invoke(ball.Id);
						}
					}
				}
			}
		}
	}

	private void RebuildBallIndexLookup()
	{
		ballIndexLookup.Clear();
		for (int i = 0; i < balls.Count; i++)
		{
			ballIndexLookup[balls[i].Id] = i;
		}
	}

	private void Log(string log)
	{
		Debug.Log("[PoolPhysics] " + log);
	}

	private Vector2 ClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
	{
		Vector2 vector = b - a;
		float value = Vector2.Dot(p - a, vector) / vector.sqrMagnitude;
		value = Mathf.Clamp01(value);
		return a + vector * value;
	}

	private bool IntersectRaySegment(Vector2 origin, Vector2 direction, Vector2 a, Vector2 b, out float distance)
	{
		distance = 0f;
		Vector2 b2 = b - a;
		float num = Cross(direction, b2);
		if (Mathf.Abs(num) <= Mathf.Epsilon)
		{
			return false;
		}
		Vector2 a2 = a - origin;
		float num2 = Cross(a2, b2) / num;
		float num3 = Cross(a2, direction) / num;
		if (num2 < 0f)
		{
			return false;
		}
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		distance = num2;
		return true;
	}

	private bool IntersectRayCircle(Vector2 origin, Vector2 direction, Vector2 center, float radius, out float distance)
	{
		distance = 0f;
		Vector2 lhs = origin - center;
		float num = Vector2.Dot(lhs, direction);
		float num2 = lhs.sqrMagnitude - radius * radius;
		if (num2 > 0f && num > 0f)
		{
			return false;
		}
		float num3 = num * num - num2;
		if (num3 < 0f)
		{
			return false;
		}
		float num4 = 0f - num - Mathf.Sqrt(num3);
		if (num4 < 0f)
		{
			num4 = 0f;
		}
		distance = num4;
		return true;
	}

	private float Cross(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}
}
