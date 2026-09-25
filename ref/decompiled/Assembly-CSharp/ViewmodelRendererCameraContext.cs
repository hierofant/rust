using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class ViewmodelRendererCameraContext : RustRendererFeatureCameraContext
{
	public Matrix4x4 prevViewProj = Matrix4x4.identity;
}
