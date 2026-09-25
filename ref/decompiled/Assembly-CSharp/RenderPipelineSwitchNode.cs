using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class RenderPipelineSwitchNode : MonoBehaviour
{
	public GameObject RustRenderPipelineObject;

	public GameObject BuiltinPipelineObject;

	private void OnAwake()
	{
		DoSwitch();
	}

	private void DoSwitch()
	{
		if (RustRenderPipeline.IsActive())
		{
			BuiltinPipelineObject.SetActive(value: false);
			RustRenderPipelineObject.SetActive(value: true);
		}
		else
		{
			RustRenderPipelineObject.SetActive(value: false);
			BuiltinPipelineObject.SetActive(value: true);
		}
	}
}
