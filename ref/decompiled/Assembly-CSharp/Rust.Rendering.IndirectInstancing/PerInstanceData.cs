using System.Runtime.InteropServices;
using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

public struct PerInstanceData
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Property
	{
		public static readonly int _ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");

		public static readonly int _PrevObjectToWorld = Shader.PropertyToID("_PrevObjectToWorld");

		public static readonly int _TextureIndex = Shader.PropertyToID("_TextureIndex");

		public static readonly int _Color = Shader.PropertyToID("_Color");

		public static readonly int _DetailColor = Shader.PropertyToID("_DetailColor");

		public static readonly int _DetailAlbedoMap_ST = Shader.PropertyToID("_DetailAlbedoMap_ST");

		public static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

		public static readonly int _MainTex_ST = Shader.PropertyToID("_MainTex_ST");
	}

	public Matrix4x4 _ObjectToWorld;

	public Matrix4x4 _PrevObjectToWorld;

	public int _TextureIndex;

	public Color _Color;

	public Color _DetailColor;

	public Vector4 _DetailAlbedoMap_ST;

	public Color _EmissionColor;

	public Vector4 _MainTex_ST;

	public float MinDistance;

	public float MaxDistance;
}
