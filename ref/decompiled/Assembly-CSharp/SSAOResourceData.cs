using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class SSAOResourceData : ContextItem
{
	public uint SampleStep;

	public TextureHandle OcclusionDepthHandle;

	public Matrix4x4 InvViewProjLeft;

	public Matrix4x4 PrevViewProjLeft;

	public Matrix4x4 PrevInvViewProjLeft;

	public float TemporalDirections;

	public float TemporalOffsets;

	private static Mesh s_quadMesh;

	public static Mesh FullscreenQuad
	{
		get
		{
			if (s_quadMesh != null)
			{
				return s_quadMesh;
			}
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			mesh.name = "SSAO Fullscreen Quad";
			mesh.vertices = new Vector3[4]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 1f, 0f),
				new Vector3(1f, 0f, 0f)
			};
			mesh.uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			};
			mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
			s_quadMesh = mesh;
			s_quadMesh.normals = Array.Empty<Vector3>();
			s_quadMesh.tangents = Array.Empty<Vector4>();
			return s_quadMesh;
		}
	}

	public override void Reset()
	{
		OcclusionDepthHandle = TextureHandle.nullHandle;
		InvViewProjLeft = Matrix4x4.identity;
		PrevViewProjLeft = Matrix4x4.identity;
		PrevInvViewProjLeft = Matrix4x4.identity;
		TemporalDirections = 0f;
		TemporalOffsets = 0f;
	}
}
