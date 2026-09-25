using System;
using System.Collections.Generic;
using Rust.UI.Toolkit.DemoControls;

public sealed class DemoControlsPlaybackSession : IDemoPropertyHost, IDemoExportHost, IDemoSceneHost, IDisposable
{
	public DemoControlsUI UI => null;

	public bool IsFfmpegAvailable => false;

	public IReadOnlyList<DemoSceneInfo> Scenes => Array.Empty<DemoSceneInfo>();

	public string CurrentSceneId => null;

	public event Action ScenesChanged
	{
		add
		{
		}
		remove
		{
		}
	}

	public void Update()
	{
	}

	public void Dispose()
	{
	}

	public bool TryGetPropertyValue(string id, out float value)
	{
		value = 0f;
		return false;
	}

	public void SetPropertyValue(string id, float value)
	{
	}

	public void StartExport(DemoExportSettings settings)
	{
	}

	public void CancelExport()
	{
	}

	public void OpenExportsFolder()
	{
	}

	public string SuggestSceneName()
	{
		return null;
	}

	public void SwitchScene(string id)
	{
	}

	public void CreateScene(string name, string cloneFromId)
	{
	}

	public void RenameScene(string id, string name)
	{
	}

	public void DeleteScene(string id)
	{
	}
}
