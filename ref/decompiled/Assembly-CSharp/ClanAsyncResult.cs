using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Facepunch;
using Rust.Assertions;

public sealed class ClanAsyncResult<T> : Pool.IPooled
{
	private readonly Stopwatch _sinceStarted = new Stopwatch();

	private AutoResetUniTaskCompletionSource<T> _completionSource;

	private bool _gotTask;

	public bool IsStarted => _sinceStarted.IsRunning;

	public float Elapsed => (float)_sinceStarted.Elapsed.TotalSeconds;

	public bool IsComplete
	{
		get
		{
			if (_completionSource != null)
			{
				return _completionSource.UnsafeGetStatus() != UniTaskStatus.Pending;
			}
			return false;
		}
	}

	public UniTask<T> Task
	{
		get
		{
			if (_completionSource == null)
			{
				throw new InvalidOperationException("Task is not started yet. Call Start() first.");
			}
			_gotTask = true;
			return _completionSource.Task;
		}
	}

	public void Start()
	{
		_sinceStarted.Restart();
		_completionSource = AutoResetUniTaskCompletionSource<T>.Create();
	}

	public bool TrySetResult(T result)
	{
		if (!_completionSource.TrySetResult(result))
		{
			return false;
		}
		_sinceStarted.Stop();
		return true;
	}

	private void Reset()
	{
		Assert.That(_gotTask, "Task was not retrieved before reset. The task must be awaited to be pooled.");
		_completionSource = null;
		_gotTask = false;
		_sinceStarted.Reset();
	}

	void Pool.IPooled.EnterPool()
	{
		Reset();
	}

	void Pool.IPooled.LeavePool()
	{
	}
}
