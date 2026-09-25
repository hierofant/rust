using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace Facepunch;

public static class Pool
{
	public interface IPooled
	{
		void EnterPool();

		void LeavePool();
	}

	public interface IPoolCollection
	{
		long ItemsCapacity { get; }

		long ItemsInStack { get; }

		long ItemsInUse { get; }

		long ItemsCreated { get; }

		long ItemsTaken { get; }

		long ItemsSpilled { get; }

		long MaxItemsInUse { get; }

		void Reset();

		void ResetMaxUsageCounter();

		void Add(object obj);

		IPoolCollection AsPool();

		IPoolCollection AsMutexPool();
	}

	public class ThreadToken
	{
		public int ThreadId;
	}

	public class PoolCollection<T> : IPoolCollection where T : class, new()
	{
		private struct Cell
		{
			public CacheLine<long> SafeAndEpoch;

			public CacheLine<object> Item;
		}

		private CacheLine<Cell[]> _cells;

		private CacheLine<long> _head;

		private CacheLine<long> _tail;

		protected CacheLine<long> _added;

		protected CacheLine<long> _removed;

		protected CacheLine<long> _itemsInUse;

		protected CacheLine<long> _itemsCreated;

		protected CacheLine<long> _itemsTaken;

		protected CacheLine<long> _itemsSpilled;

		protected CacheLine<long> _maxItemsInUse;

		private const int MaxAttempts = 1000;

		public long ItemsCapacity { get; protected set; }

		public long ItemsInStack => Volatile.Read(ref _added.Value) - Volatile.Read(ref _removed.Value);

		public long ItemsInUse => _itemsInUse.Value;

		public long ItemsCreated => _itemsCreated.Value;

		public long ItemsTaken => _itemsTaken.Value;

		public long ItemsSpilled => _itemsSpilled.Value;

		public long MaxItemsInUse => _maxItemsInUse.Value;

		public PoolCollection()
		{
			Resize(512);
		}

		public void Reset()
		{
			Resize((int)ItemsCapacity);
		}

		public void ResetMaxUsageCounter()
		{
			_maxItemsInUse = _itemsInUse;
		}

		public virtual void Resize(int size)
		{
			int processorCount = Environment.ProcessorCount;
			_cells.Value = new Cell[size + processorCount * 2];
			_head.Value = (_tail.Value = _cells.Value.Length);
			ItemsCapacity = size;
			_itemsInUse.Value = 0L;
			_itemsCreated.Value = 0L;
			_itemsTaken.Value = 0L;
			_itemsSpilled.Value = 0L;
			_maxItemsInUse.Value = 0L;
			_added.Value = 0L;
			_removed.Value = 0L;
		}

		private bool TryAdd(T obj)
		{
			ThreadToken tlsToken = TlsToken;
			Cell[] value = _cells.Value;
			int num = value.Length;
			for (int i = 0; i < 1000; i++)
			{
				if (ItemsInStack >= ItemsCapacity)
				{
					break;
				}
				long num2;
				do
				{
					num2 = Interlocked.Increment(ref _tail.Value) - 1;
					long num3 = num2 / num;
					int num4 = (int)(num2 % num);
					ref Cell reference = ref value[num4];
					long num5 = Volatile.Read(ref reference.SafeAndEpoch.Value);
					bool flag = (num5 & long.MinValue) != 0;
					long num6 = num5 & 0x7FFFFFFFFFFFFFFFL;
					object obj2 = Volatile.Read(ref reference.Item.Value);
					if ((obj2 == null || obj2 is ThreadToken) && num6 < num3 && (flag || Volatile.Read(ref _head.Value) <= num2) && Interlocked.CompareExchange(ref reference.Item.Value, tlsToken, obj2) == obj2)
					{
						long value2 = num3 | long.MinValue;
						if (Interlocked.CompareExchange(ref reference.SafeAndEpoch.Value, value2, num5) != num5)
						{
							Interlocked.CompareExchange(ref reference.Item.Value, null, tlsToken);
						}
						else if (Interlocked.CompareExchange(ref reference.Item.Value, obj, tlsToken) == tlsToken)
						{
							Interlocked.Increment(ref _added.Value);
							return true;
						}
					}
				}
				while (num2 - Volatile.Read(ref _head.Value) < num);
			}
			return false;
		}

		private bool TryTake(out T obj)
		{
			Cell[] value = _cells.Value;
			int num = value.Length;
			for (int i = 0; i < 1000; i++)
			{
				if (ItemsInStack <= 0)
				{
					break;
				}
				long num2;
				do
				{
					num2 = Interlocked.Increment(ref _head.Value) - 1;
					long num3 = num2 / num;
					int num4 = (int)(num2 % num);
					while (true)
					{
						ref Cell reference = ref value[num4];
						long num5 = Volatile.Read(ref reference.SafeAndEpoch.Value);
						bool flag = (num5 & long.MinValue) != 0;
						long num6 = num5 & 0x7FFFFFFFFFFFFFFFL;
						object obj2 = Volatile.Read(ref reference.Item.Value);
						if (num5 != Volatile.Read(ref reference.SafeAndEpoch.Value))
						{
							continue;
						}
						if (num6 == num3 && obj2 != null && !(obj2 is ThreadToken))
						{
							Volatile.Write(ref reference.Item.Value, null);
							obj = obj2 as T;
							Interlocked.Increment(ref _removed.Value);
							return true;
						}
						if (num6 <= num3 && (obj2 == null || obj2 is ThreadToken))
						{
							if (!(obj2 is ThreadToken) || Interlocked.CompareExchange(ref reference.Item.Value, null, obj2) == obj2)
							{
								long value2 = num3 | (flag ? long.MinValue : 0);
								if (Interlocked.CompareExchange(ref reference.SafeAndEpoch.Value, value2, num5) == num5)
								{
									break;
								}
							}
							continue;
						}
						if (num6 >= num3 || obj2 == null || obj2 is ThreadToken)
						{
							break;
						}
						long value3 = num6;
						if (Interlocked.CompareExchange(ref reference.SafeAndEpoch.Value, value3, num5) == num5)
						{
							break;
						}
					}
				}
				while (Volatile.Read(ref _tail.Value) > num2 + 1);
			}
			obj = null;
			return false;
		}

		public virtual void Add(T obj)
		{
			if (obj is IPooled pooled)
			{
				pooled.EnterPool();
			}
			Interlocked.Decrement(ref _itemsInUse.Value);
			if (!TryAdd(obj))
			{
				Interlocked.Increment(ref _itemsSpilled.Value);
			}
		}

		public virtual T Take()
		{
			long num = Interlocked.Increment(ref _itemsInUse.Value);
			long num2;
			do
			{
				num2 = Interlocked.Read(ref _maxItemsInUse.Value);
			}
			while (num >= num2 && Interlocked.CompareExchange(ref _maxItemsInUse.Value, num, num2) != num2);
			T obj = null;
			if (TryTake(out obj))
			{
				Interlocked.Increment(ref _itemsTaken.Value);
			}
			else
			{
				obj = new T();
				Interlocked.Increment(ref _itemsCreated.Value);
			}
			if (obj is IPooled pooled)
			{
				pooled.LeavePool();
			}
			return obj;
		}

		public void Fill()
		{
			long num = ItemsCapacity - ItemsInStack;
			for (int i = 0; i < num; i++)
			{
				Add(new T());
				_itemsInUse.Value++;
			}
		}

		void IPoolCollection.Add(object obj)
		{
			Add((T)obj);
		}

		void IPoolCollection.Reset()
		{
			Resize((int)ItemsCapacity);
		}

		void IPoolCollection.ResetMaxUsageCounter()
		{
			_maxItemsInUse.Value = 0L;
		}

		public virtual IPoolCollection AsPool()
		{
			return this;
		}

		public virtual IPoolCollection AsMutexPool()
		{
			MutexPoolCollection<T> mutexPoolCollection = new MutexPoolCollection<T>();
			mutexPoolCollection.Resize((int)ItemsCapacity);
			return mutexPoolCollection;
		}
	}

	internal class MutexPoolCollection<T> : PoolCollection<T> where T : class, new()
	{
		private object _lock = new object();

		private BufferList<T> _buffer;

		private HashSet<T> _hashset;

		public override void Resize(int size)
		{
			base.Resize(0);
			lock (_lock)
			{
				_buffer = new BufferList<T>(size);
				_hashset = new HashSet<T>(size);
				base.ItemsCapacity = size;
			}
		}

		public override void Add(T obj)
		{
			if (obj is IPooled pooled)
			{
				pooled.EnterPool();
			}
			lock (_lock)
			{
				_itemsInUse.Value--;
				if (base.ItemsInStack < base.ItemsCapacity)
				{
					_buffer.Push(obj);
					_added.Value++;
				}
				else
				{
					_itemsSpilled.Value++;
				}
			}
		}

		public override T Take()
		{
			T val;
			lock (_lock)
			{
				_itemsInUse.Value++;
				_maxItemsInUse.Value = Math.Max(base.ItemsInUse, base.MaxItemsInUse);
				if (base.ItemsInStack > 0)
				{
					val = _buffer.Pop();
					_removed.Value++;
					_itemsTaken.Value++;
				}
				else
				{
					val = new T();
					_itemsCreated.Value++;
				}
			}
			if (val is IPooled pooled)
			{
				pooled.LeavePool();
			}
			return val;
		}

		public override IPoolCollection AsPool()
		{
			PoolCollection<T> poolCollection = new PoolCollection<T>();
			poolCollection.Resize((int)base.ItemsCapacity);
			return poolCollection;
		}

		public override IPoolCollection AsMutexPool()
		{
			return this;
		}
	}

	public static bool UseMutexPool;

	[ThreadStatic]
	private static ThreadToken _tlsToken;

	public static ConcurrentDictionary<Type, IPoolCollection> Directory = new ConcurrentDictionary<Type, IPoolCollection>();

	private static ThreadToken TlsToken
	{
		get
		{
			if (_tlsToken == null)
			{
				_tlsToken = new ThreadToken
				{
					ThreadId = Environment.CurrentManagedThreadId
				};
			}
			return _tlsToken;
		}
	}

	public static void Free<T>(ref T obj) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		FreeInternal(ref obj);
	}

	public static void Free<T>(ref List<T> obj, bool freeElements = false) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					T obj2 = item;
					Free(ref obj2);
				}
			}
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void Free<T>(ref HashSet<T> obj, bool freeElements = false) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					T obj2 = item;
					Free(ref obj2);
				}
			}
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void Free<TKey, TVal>(ref Dictionary<TKey, TVal> dict, bool freeElements = false) where TVal : class, IPooled, new()
	{
		if (dict == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (KeyValuePair<TKey, TVal> item in dict)
			{
				if (item.Value != null)
				{
					TVal obj = item.Value;
					Free(ref obj);
				}
			}
		}
		dict.Clear();
		FreeInternal(ref dict);
	}

	public static void Free<T>(ref BufferList<T> obj, bool freeElements = false) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					T obj2 = item;
					Free(ref obj2);
				}
			}
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void Free<TKey, TVal>(ref ListDictionary<TKey, TVal> dict, bool freeElements = false) where TVal : class, IPooled, new()
	{
		if (dict == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			for (int i = 0; i < dict.Values.Count; i++)
			{
				TVal obj = dict.Values[i];
				if (obj != null)
				{
					Free(ref obj);
				}
			}
		}
		dict.Clear();
		FreeInternal(ref dict);
	}

	public static void Free<T>(ref Queue<T> obj, bool freeElements = false) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					T obj2 = item;
					Free(ref obj2);
				}
			}
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void Free<T>(ref ListHashSet<T> obj, bool freeElements = false) where T : class, IPooled, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		if (freeElements)
		{
			foreach (T item in obj)
			{
				if (item != null)
				{
					T obj2 = item;
					Free(ref obj2);
				}
			}
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnsafe<T>(ref T obj) where T : class, new()
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged(ref MemoryStream obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.SetLength(0L);
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged(ref StringBuilder obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged(ref Stopwatch obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Reset();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged<T>(ref List<T> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged<T>(ref HashSet<T> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged<TKey, TVal>(ref Dictionary<TKey, TVal> dict)
	{
		if (dict == null)
		{
			throw new ArgumentNullException();
		}
		dict.Clear();
		FreeInternal(ref dict);
	}

	public static void FreeUnmanaged<T>(ref BufferList<T> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged<TKey, TVal>(ref ListDictionary<TKey, TVal> dict)
	{
		if (dict == null)
		{
			throw new ArgumentNullException();
		}
		dict.Clear();
		FreeInternal(ref dict);
	}

	public static void FreeUnmanaged<T>(ref Queue<T> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	public static void FreeUnmanaged<T>(ref ListHashSet<T> obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException();
		}
		obj.Clear();
		FreeInternal(ref obj);
	}

	private static void FreeInternal<T>(ref T obj) where T : class, new()
	{
		FindCollection<T>().Add(obj);
		obj = null;
	}

	public static T Get<T>() where T : class, new()
	{
		return FindCollection<T>().Take();
	}

	public static void ResizeBuffer<T>(int size) where T : class, new()
	{
		FindCollection<T>().Resize(size);
	}

	public static void FillBuffer<T>() where T : class, new()
	{
		FindCollection<T>().Fill();
	}

	public static PoolCollection<T> FindCollection<T>() where T : class, new()
	{
		return Pool<T>.Collection;
	}

	public static void Clear(string filter = null)
	{
		if (string.IsNullOrEmpty(filter))
		{
			foreach (KeyValuePair<Type, IPoolCollection> item in Directory)
			{
				item.Value.Reset();
			}
			return;
		}
		foreach (KeyValuePair<Type, IPoolCollection> item2 in Directory)
		{
			if (item2.Key.FullName.Contains(filter, CompareOptions.IgnoreCase))
			{
				item2.Value.Reset();
			}
		}
	}

	private static bool Contains(this string haystack, string needle, CompareOptions options)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, options) >= 0;
	}
}
internal static class Pool<T> where T : class, new()
{
	private static Pool.PoolCollection<T> _collection;

	private static bool _usingMutexPool;

	public static Pool.PoolCollection<T> Collection
	{
		get
		{
			if (_usingMutexPool != Pool.UseMutexPool)
			{
				Pool.IPoolCollection poolCollection = (Pool.UseMutexPool ? _collection.AsMutexPool() : _collection.AsPool());
				if (Pool.Directory.TryUpdate(typeof(T), poolCollection, _collection))
				{
					_collection = (Pool.PoolCollection<T>)poolCollection;
				}
				_usingMutexPool = Pool.UseMutexPool;
			}
			return _collection;
		}
	}

	static Pool()
	{
		if (Pool.UseMutexPool)
		{
			_collection = new Pool.MutexPoolCollection<T>();
		}
		else
		{
			_collection = new Pool.PoolCollection<T>();
		}
		_usingMutexPool = Pool.UseMutexPool;
		Pool.Directory[typeof(T)] = _collection;
	}
}
