using UnityEngine;
using UnityEngine.Rendering;

public static class ShaderEx
{
	public static bool SetTextureEx(this ComputeShader shader, int kernelIndex, string name, Texture texture)
	{
		if (texture == null)
		{
			return false;
		}
		shader.SetTexture(kernelIndex, name, texture);
		return true;
	}

	public static bool SetTextureEx(this ComputeShader shader, int kernelIndex, int id, Texture texture)
	{
		if (texture == null)
		{
			return false;
		}
		shader.SetTexture(kernelIndex, id, texture);
		return true;
	}

	public static bool SetComputeTextureParamEx(this CommandBuffer cb, ComputeShader shader, int kernelIndex, string name, Texture texture)
	{
		if (texture == null)
		{
			return false;
		}
		cb.SetComputeTextureParam(shader, kernelIndex, name, texture);
		return true;
	}
}
