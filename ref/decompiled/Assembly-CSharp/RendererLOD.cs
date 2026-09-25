using System;
using Rust.Rendering.IndirectInstancing;
using UnityEngine;
using UnityEngine.Rendering;

public class RendererLOD : InstancedLODComponent, IPrefabPreProcess, ICustomMaterialReplacer, IHLODMeshSource
{
	[Serializable]
	public class State
	{
		public float distance;

		public Renderer renderer;

		[NonSerialized]
		public MeshFilter filter;

		[NonSerialized]
		public ShadowCastingMode shadowMode;

		[NonSerialized]
		public bool isImpostor;

		[ReadOnly]
		public bool hasCached;

		[ReadOnly]
		public Mesh stateMesh;

		[ReadOnly]
		public Material[] stateMaterials;

		[ReadOnly]
		public Material[] defaultMaterials;

		[ReadOnly]
		public ShadowCastingMode cachedShadowMode;

		[ReadOnly]
		public MotionVectorGenerationMode motionVectors;
	}

	public float minDistanceMultiplier;

	public State[] States = Array.Empty<State>();

	public bool shouldNotifyOnLODChange;

	[ReadOnly]
	public MeshRenderer collapsedRenderer;

	[ReadOnly]
	public MeshFilter collapsedFilter;

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public Mesh GetFinalLodMesh(out Matrix4x4 localToWorldMatrix)
	{
		localToWorldMatrix = base.transform.localToWorldMatrix;
		for (int num = States.Length - 1; num >= 0; num--)
		{
			Mesh mesh = null;
			if (States[num].renderer != null && States[num].renderer.TryGetComponent<MeshFilter>(out var component))
			{
				mesh = component.sharedMesh;
			}
			if (mesh != null)
			{
				localToWorldMatrix = States[num].renderer.transform.localToWorldMatrix;
				return mesh;
			}
		}
		return null;
	}

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
	}
}
