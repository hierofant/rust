using System;
using System.Collections.Generic;

public class Memoized<TResult, TArgs>
{
	private readonly Func<TArgs, TResult> _factory;

	private readonly Dictionary<TArgs, TResult> _cache;

	public Memoized(Func<TArgs, TResult> factory)
	{
		_factory = factory ?? throw new ArgumentNullException("factory");
		_cache = new Dictionary<TArgs, TResult>();
	}

	public TResult Get(TArgs args)
	{
		if (_cache.TryGetValue(args, out var value))
		{
			return value;
		}
		TResult val = _factory(args);
		_cache.Add(args, val);
		return val;
	}
}
public static class Memoized
{
	public static readonly Memoized<string, int> IntToString = new Memoized<string, int>((int i) => i.ToString());
}
