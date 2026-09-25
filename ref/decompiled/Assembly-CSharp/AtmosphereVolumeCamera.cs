using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class AtmosphereVolumeCamera : RustRendererFeatureCamera<AtmosphereVolumeCameraContext>
{
	public override bool OnBeginRendering()
	{
		Shader.SetGlobalVector("_SceneFogMode", Vector4.zero);
		return true;
	}

	public override void OnEndRendering()
	{
		Shader.SetGlobalVector("_SceneFogMode", Vector4.zero);
	}
}
