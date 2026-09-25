using System;
using UnityEngine;

public class DoorTextureScroll : FacepunchBehaviour
{
	[Serializable]
	public class RendererMaterialIndex
	{
		public Renderer renderer;

		public int materialIndex;
	}

	public Door doorEntity;

	public Animator animator;

	public Transform followTransform;

	public RendererMaterialIndex[] rendererMaterialIndices;

	public float scrollSpeed = 0.1f;

	public bool scrollX;

	public bool scrollY;
}
