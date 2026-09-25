using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RenderTexturePool
{
	private readonly Queue<RenderTexture> inactive = new Queue<RenderTexture>();

	private readonly HashSet<RenderTexture> active = new HashSet<RenderTexture>();

	public RenderTexturePool(int width, int height, GraphicsFormat graphicsFormat, TextureDimension textureDimension, FilterMode filterMode, int capacity)
	{
		for (int i = 0; i < capacity; i++)
		{
			RenderTexture renderTexture = new RenderTexture(width, height, 0, graphicsFormat, 0)
			{
				dimension = textureDimension,
				filterMode = filterMode,
				anisoLevel = 0
			};
			renderTexture.Create();
			inactive.Enqueue(renderTexture);
		}
	}

	public RenderTexture GetInstance()
	{
		if (inactive.Count <= 0)
		{
			return null;
		}
		RenderTexture renderTexture = inactive.Dequeue();
		active.Add(renderTexture);
		return renderTexture;
	}

	public void ReleaseInstance(RenderTexture renderTexture)
	{
		if (!(renderTexture == null))
		{
			active.Remove(renderTexture);
			inactive.Enqueue(renderTexture);
		}
	}
}
