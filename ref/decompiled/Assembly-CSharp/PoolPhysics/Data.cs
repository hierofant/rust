using System;
using UnityEngine;

namespace PoolPhysics;

public class Data
{
	[Serializable]
	public struct Ball
	{
		public int Id;

		public Vector2 Position;

		public Vector2 Velocity;

		public float Radius;

		public bool IsKinematic;
	}

	[Serializable]
	public struct Wall
	{
		public Vector2 A;

		public Vector2 B;

		public Vector2 Normal;
	}

	[Serializable]
	public struct Pocket
	{
		public Vector2 Position;

		public float Radius;
	}

	[Serializable]
	public struct Table
	{
		public Vector2 Dimensions;
	}
}
