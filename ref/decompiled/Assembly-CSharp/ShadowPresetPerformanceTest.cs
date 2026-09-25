using System.Text;
using System.Threading.Tasks;
using ConVar;
using UnityEngine;

public static class ShadowPresetPerformanceTest
{
	private struct TestResultAverages
	{
		public float totalCpuTime;

		public float totalGpuTime;

		public float cpuRenderThreadTime;

		public float cpuMainThreadPresentWaitTime;
	}

	private const int FRAMES_TO_CAPTURE = 100;

	public static async void RunAll()
	{
		Debug.Log("Shadow Preset Performance Test Started...");
		int originalPreset = GraphicsSettings.shadowQualityPreset;
		StringBuilder performanceLog = new StringBuilder();
		for (int i = 0; i < 4; i++)
		{
			ChangePreset(i);
			await Task.Delay(3000);
			ResultsToStringBuilder(CaptureTimings(), performanceLog);
		}
		ChangePreset(originalPreset);
		Debug.Log(performanceLog.ToString());
	}

	public static void RunTestWithCurrentPreset()
	{
		TestResultAverages resultAverages = CaptureTimings();
		StringBuilder stringBuilder = new StringBuilder();
		ResultsToStringBuilder(resultAverages, stringBuilder);
		Debug.Log(stringBuilder.ToString());
	}

	private static void ResultsToStringBuilder(TestResultAverages resultAverages, StringBuilder performanceLog)
	{
		performanceLog.AppendLine("Timings for Shadow Preset " + GraphicsSettings.shadowQualityPreset + ":");
		performanceLog.AppendLine($"CPU Time: {resultAverages.totalCpuTime} ms");
		performanceLog.AppendLine($"GPU Time: {resultAverages.totalGpuTime} ms");
		performanceLog.AppendLine($"CPU Render Thread Time: {resultAverages.cpuRenderThreadTime} ms");
		performanceLog.AppendLine($"CPU Main Thread Present Wait Time: {resultAverages.cpuMainThreadPresentWaitTime} ms");
	}

	public static void ChangePreset(int value)
	{
		ConsoleSystem.Run(ConsoleSystem.Option.Client.Quiet(), "graphicssettings.shadowqualitypreset", value);
	}

	private static TestResultAverages CaptureTimings()
	{
		FrameTiming[] array = new FrameTiming[100];
		FrameTimingManager.CaptureFrameTimings();
		FrameTimingManager.GetLatestTimings(100u, array);
		TestResultAverages testResultAverages = default(TestResultAverages);
		testResultAverages.totalCpuTime = 0f;
		testResultAverages.totalGpuTime = 0f;
		TestResultAverages result = testResultAverages;
		FrameTiming[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			FrameTiming frameTiming = array2[i];
			result.totalCpuTime += (float)frameTiming.cpuFrameTime;
			result.totalGpuTime += (float)frameTiming.cpuFrameTime;
			result.cpuRenderThreadTime += (float)frameTiming.cpuRenderThreadFrameTime;
			result.cpuMainThreadPresentWaitTime = (float)frameTiming.cpuMainThreadPresentWaitTime;
		}
		result.totalCpuTime /= array.Length;
		result.totalGpuTime /= array.Length;
		result.cpuRenderThreadTime /= array.Length;
		result.cpuMainThreadPresentWaitTime /= array.Length;
		return result;
	}
}
