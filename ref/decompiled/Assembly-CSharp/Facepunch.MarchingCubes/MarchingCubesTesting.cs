using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

public class MarchingCubesTesting : FacepunchBehaviour, IDisposable
{
	public MeshFilter TargetFilter;

	public MeshCollider TargetCollider;

	public Vector3Int GridResolution = new Vector3Int(32, 40, 32);

	public Vector3 GridOffset = new Vector3(0f, -20f, 0f);

	public float GridScale = 1f / 32f;

	private Mesh _mesh;

	private MarchingCubesGenerator _generator;

	private bool _hasInit;

	public void Init()
	{
		if (_mesh == null)
		{
			_mesh = new Mesh
			{
				name = "MarchingCubesTestingMesh"
			};
		}
		TargetFilter.sharedMesh = _mesh;
		_generator = new MarchingCubesGenerator(_mesh, _mesh, null, GridOffset, GridScale);
	}

	public void RegenerateWithFloats(SDFSet set, float iso, Stopwatch sw = null)
	{
		Init();
		set.CompleteDataJobs();
		NativeList<float3> vertices = default(NativeList<float3>);
		NativeList<int> indices = default(NativeList<int>);
		sw?.Restart();
		foreach (SDFChunk chunk in set.Chunks)
		{
			if (vertices.IsCreated)
			{
				vertices.Dispose();
			}
			if (indices.IsCreated)
			{
				indices.Dispose();
			}
			_generator.ScheduleSDFMarch(chunk.DataArray, iso, out vertices, out indices, default(JobHandle)).Complete();
		}
		sw?.Stop();
		_generator.ScheduleMeshWrite(vertices, indices, out var meshData, withNormals: true, default(JobHandle)).Complete();
		_generator.ApplyMeshData(meshData, _generator.Mesh);
		TargetCollider.sharedMesh = _mesh;
		vertices.Dispose();
		indices.Dispose();
		UnityEngine.Debug.Log($"mesh v: {_mesh.vertexCount} t: {_mesh.triangles.Length / 3}");
		Dispose();
	}

	public void Dispose()
	{
		_generator.Dispose();
	}
}
