using System;
using Facepunch.Extend;
using UnityEngine;

[RequireComponent(typeof(TerrainMeta))]
public abstract class TerrainExtension : MonoBehaviour
{
	[NonSerialized]
	public bool isInitialized;

	internal TerrainData terrainData;

	internal TerrainRenderer terrainRenderer;

	internal TerrainConfig config;

	public void Init(TerrainRenderer terrainRenderer, TerrainData terrainData, TerrainConfig config)
	{
		this.terrainRenderer = terrainRenderer;
		this.terrainData = terrainData;
		this.config = config;
	}

	public virtual void Setup()
	{
	}

	public virtual void PostSetup()
	{
	}

	public void LogSize(object obj, ulong size)
	{
		Debug.Log(obj.GetType()?.ToString() + " allocated: " + size.FormatBytes());
	}
}
