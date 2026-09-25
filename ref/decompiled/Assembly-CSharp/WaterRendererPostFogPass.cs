using System;
using Rust.RenderPipeline.Runtime;
using Rust.RenderPipeline.Runtime.RenderingContext;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public class WaterRendererPostFogPass : RustRenderPass
{
	public class PassData
	{
		public TextureHandle srcHandle;

		public TextureHandle dstHandle;

		public TextureHandle ssrReflectionHandle;

		public RenderTargetIdentifier src;

		public RenderTargetIdentifier dst;

		public RenderTargetIdentifier ssrReflection;

		public Material multiCopyMat;

		public Material reflectionMat;

		public Matrix4x4 ssrProj;
	}

	private readonly Material _multiCopyMat;

	private readonly Material _reflectionMat;

	public static TextureDesc BackgroundTextureDesc = new TextureDesc(0, 0)
	{
		name = "Water Post Fog Background Texture",
		colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
		wrapMode = TextureWrapMode.Clamp,
		filterMode = FilterMode.Bilinear
	};

	public static TextureDesc SSRTextureDesc = new TextureDesc(0, 0)
	{
		name = "Water SSR Texture",
		colorFormat = GraphicsFormat.R8G8B8A8_SRGB,
		filterMode = FilterMode.Bilinear,
		wrapMode = TextureWrapMode.Clamp
	};

	public WaterRendererPostFogPass(Material multiCopyMat, Material reflectionMat)
	{
		_multiCopyMat = multiCopyMat;
		_reflectionMat = reflectionMat;
	}

	public static void Draw(CommandBuffer cmd, PassData data)
	{
		cmd.Blit(data.src, data.dst, data.multiCopyMat, 1);
		cmd.SetGlobalTexture(WaterRendererShaderProps.BackgroundColorTexture, data.dst);
		cmd.SetGlobalMatrix(WaterRendererShaderProps.WaterSSR_CameraProj, data.ssrProj);
		cmd.Blit(null, data.ssrReflection, data.reflectionMat, WaterSystem.Instance.Reflections);
		cmd.SetGlobalTexture(WaterRendererShaderProps.WaterSSR_ReflectionTexture, data.ssrReflection);
	}

	public static void ExecutePass(PassData data, RenderGraphContext ctx)
	{
		data.src = data.srcHandle;
		data.dst = data.dstHandle;
		data.ssrReflection = data.ssrReflectionHandle;
		Draw(ctx.cmd, data);
	}

	public static Matrix4x4 ComputeCameraSSRProj(int width, int height, Matrix4x4 projMat)
	{
		return Matrix4x4.Scale(new Vector3(width, height, 1f)) * Matrix4x4.TRS(new Vector3(0.5f, 0.5f, 0f), Quaternion.identity, new Vector3(0.5f, 0.5f, 1f)) * GL.GetGPUProjectionMatrix(projMat, renderIntoTexture: false);
	}

	public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData, CameraFeatureContexts featureContexts)
	{
		if (WaterSystem.Instance.Reflections <= 0)
		{
			return;
		}
		RustResourceDataContext rustResourceDataContext = frameData.Get<RustResourceDataContext>();
		RustCameraContext rustCameraContext = frameData.Get<RustCameraContext>();
		WaterRendererFrameData waterRendererFrameData = frameData.Get<WaterRendererFrameData>();
		PostOpaqueDepthResourceData postOpaqueDepthResourceData = frameData.Get<PostOpaqueDepthResourceData>();
		int x = rustCameraContext.CameraBufferSize.x;
		int y = rustCameraContext.CameraBufferSize.y;
		PassData passData;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PassData>("Water Post Fog", out passData, "D:\\ws\\workspace\\Rust-Server-release\\Assets\\Scripts\\Rendering\\RendererFeatures\\Water\\WaterRendererPostFogPass.cs", 99);
		try
		{
			Matrix4x4 ssrProj = ComputeCameraSSRProj(rustCameraContext.CameraBufferSize.x, rustCameraContext.CameraBufferSize.y, rustCameraContext.ProjectionMatrix);
			passData.ssrProj = ssrProj;
			passData.multiCopyMat = _multiCopyMat;
			passData.reflectionMat = _reflectionMat;
			PassData passData2 = passData;
			TextureHandle input = rustResourceDataContext.ActiveColorTexture;
			passData2.srcHandle = renderGraphBuilder.ReadTexture(in input);
			renderGraphBuilder.ReadTexture(in postOpaqueDepthResourceData.postOpaqueDepthHandle);
			input = rustResourceDataContext.CameraDepthTexture;
			renderGraphBuilder.ReadTexture(in input);
			PassData passData3 = passData;
			TextureDesc desc = RustRenderPipelineUtils.TextureDescSetSize(BackgroundTextureDesc, x / 2, y / 2);
			input = renderGraph.CreateTexture(in desc);
			passData3.dstHandle = (waterRendererFrameData.backgroundColorTex = renderGraphBuilder.WriteTexture(in input));
			PassData passData4 = passData;
			desc = RustRenderPipelineUtils.TextureDescSetSize(SSRTextureDesc, x, y);
			input = renderGraph.CreateTexture(in desc);
			passData4.ssrReflectionHandle = (waterRendererFrameData.ssrReflectionTex = renderGraphBuilder.WriteTexture(in input));
			renderGraphBuilder.SetRenderFunc(delegate(PassData pd, RenderGraphContext ctx)
			{
				ExecutePass(pd, ctx);
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}
}
