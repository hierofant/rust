using System;
using Rust.Rendering.IndirectInstancing;
using UnityEngine;

public class MeshLOD : InstancedLODComponent, IHLODMeshSource
{
	[Serializable]
	public class State
	{
		[Range(1f, 1000f)]
		public float distance;

		public Mesh mesh;

		public bool disableShadows;
	}

	public State[] States;

	public Mesh GetHighestDetailMesh()
	{
		if (States != null && States.Length != 0)
		{
			return States[0].mesh;
		}
		return null;
	}
}
