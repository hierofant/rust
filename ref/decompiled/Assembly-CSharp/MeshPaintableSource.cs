using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class MeshPaintableSource : MonoBehaviour, IClientComponent
{
	public Vector4 uvRange = new Vector4(0f, 0f, 1f, 1f);

	public int texWidth = 256;

	public int texHeight = 128;

	public string replacementTextureName = "_DecalTexture";

	public float cameraFOV = 60f;

	public float cameraDistance = 2f;

	[NonSerialized]
	public Texture2D texture;

	public GameObject sourceObject;

	public Mesh collisionMesh;

	public Vector3 localPosition;

	public Vector3 localRotation;

	public bool applyToAllRenderers = true;

	public Renderer[] extraRenderers;

	public List<Renderer> ignoreRenderers;

	public bool paint3D;

	public bool applyToSkinRenderers = true;

	public bool applyToFirstPersonLegs = true;

	[NonSerialized]
	public bool isSelected;

	[NonSerialized]
	public Renderer legRenderer;

	private static MaterialPropertyBlock block;

	public void Init()
	{
		if (texture == null)
		{
			texture = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, mipChain: false);
			texture.name = "MeshPaintableSource_" + base.gameObject.name;
			texture.wrapMode = TextureWrapMode.Clamp;
			texture.Clear(Color.clear);
		}
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		else
		{
			block.Clear();
		}
		UpdateMaterials(block, null, forEditing: false, isSelected);
		ApplyPropertyBlockToAllRenderers();
		base.gameObject.BroadcastRefresh();
	}

	private void ApplyPropertyBlockToAllRenderers()
	{
		List<Renderer> obj = Pool.Get<List<Renderer>>();
		Transform transform = (applyToAllRenderers ? base.transform.root : base.transform);
		if (applyToSkinRenderers)
		{
			BaseEntity componentInParent = GetComponentInParent<BaseEntity>(includeInactive: true);
			if (componentInParent != null)
			{
				transform = componentInParent.transform;
			}
		}
		transform.GetComponentsInChildren(includeInactive: true, obj);
		foreach (Renderer item in obj)
		{
			if (!ignoreRenderers.Contains(item) && (applyToSkinRenderers || !item.TryGetComponent<PlayerModelSkin>(out var _)))
			{
				item.SetPropertyBlock(block);
			}
		}
		if (extraRenderers != null)
		{
			Renderer[] array = extraRenderers;
			foreach (Renderer renderer in array)
			{
				if (renderer != null)
				{
					renderer.SetPropertyBlock(block);
					renderer.gameObject.BroadcastRefresh();
				}
			}
		}
		if (applyToFirstPersonLegs && legRenderer != null)
		{
			legRenderer.SetPropertyBlock(block);
			legRenderer.gameObject.BroadcastRefresh();
		}
		Pool.FreeUnmanaged(ref obj);
	}

	public void Free()
	{
		if ((bool)texture)
		{
			UnityEngine.Object.Destroy(texture);
			texture = null;
		}
		if (block != null)
		{
			block.Clear();
		}
		else
		{
			block = new MaterialPropertyBlock();
		}
		ApplyPropertyBlockToAllRenderers();
	}

	public void OnDestroy()
	{
		Free();
	}

	public virtual void UpdateMaterials(MaterialPropertyBlock block, Texture2D textureOverride = null, bool forEditing = false, bool isSelected = false)
	{
		block.SetTexture(replacementTextureName, textureOverride ?? texture);
	}

	public virtual Color32[] UpdateFrom(Texture2D input)
	{
		Init();
		Color32[] pixels = input.GetPixels32();
		texture.SetPixels32(pixels);
		texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		return pixels;
	}

	public void Load(byte[] data)
	{
		Init();
		if (data != null)
		{
			texture.LoadImage(data);
			texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		}
	}

	public void Clear()
	{
		if (!(texture == null))
		{
			texture.Clear(new Color(0f, 0f, 0f, 0f));
			texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		}
	}
}
