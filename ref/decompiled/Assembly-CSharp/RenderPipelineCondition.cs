using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class RenderPipelineCondition : MonoBehaviour
{
	public enum RenderPipelineType
	{
		RustRenderPipeline,
		BuiltinRenderPipeline
	}

	public RenderPipelineType renderPipeline;

	private void Awake()
	{
		bool flag = RustRenderPipeline.IsActive();
		if (renderPipeline == RenderPipelineType.RustRenderPipeline)
		{
			base.gameObject.SetActive(flag);
		}
		else
		{
			base.gameObject.SetActive(!flag);
		}
	}
}
