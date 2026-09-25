using System.IO;
using Rust.Water5;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ocean Settings", menuName = "Water5/Ocean Settings")]
public class OceanSettings : ScriptableObject
{
	[Header("Compute Shaders")]
	public ComputeShader waveSpectrumCompute;

	public ComputeShader fftCompute;

	public ComputeShader waveMergeCompute;

	public ComputeShader waveInitialSpectrum;

	[Header("Global Ocean Params")]
	public float[] octaveScales;

	public float lamda;

	public float windDirection;

	public float distanceAttenuationFactor;

	public float depthAttenuationFactor;

	[Header("Ocean Spectra")]
	public OceanSpectrumSettings[] spectrumSettings;

	[HideInInspector]
	public float[] spectrumRanges;

	public unsafe OceanDisplacementShort3[,,] LoadSimData()
	{
		OceanDisplacementShort3[,,] array = new OceanDisplacementShort3[spectrumSettings.Length, 72, 65536];
		string path = Application.streamingAssetsPath + "/" + base.name + ".physicsdata.dat";
		if (!File.Exists(path))
		{
			Debug.Log("Simulation Data not found");
			return array;
		}
		byte[] array2 = File.ReadAllBytes(path);
		fixed (byte* source = array2)
		{
			fixed (OceanDisplacementShort3* destination = array)
			{
				UnsafeUtility.MemCpy(destination, source, array2.Length);
			}
		}
		return array;
	}

	internal unsafe Rust.Water5.NativeOceanDisplacementShort3 LoadNativeSimData()
	{
		Rust.Water5.NativeOceanDisplacementShort3 result = Rust.Water5.NativeOceanDisplacementShort3.Create(spectrumSettings.Length, 72, 65536);
		string text = Application.streamingAssetsPath + "/" + base.name + ".physicsdata.dat";
		if (!File.Exists(text))
		{
			Debug.Log("Simulation Data not found");
			return result;
		}
		NativeArray<byte> nativeArray = FileEx.ReadAllBytesNative(text, Allocator.Temp);
		void* unsafePtr = result.GetNativeRaw().GetUnsafePtr();
		void* unsafePtr2 = nativeArray.GetUnsafePtr();
		UnsafeUtility.MemCpy(unsafePtr, unsafePtr2, nativeArray.Length);
		return result;
	}
}
